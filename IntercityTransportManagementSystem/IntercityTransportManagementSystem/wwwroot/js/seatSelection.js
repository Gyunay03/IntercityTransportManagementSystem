import { connection } from './reservationHubClient.js';

connection.on("SeatLocked", function (data) {
	if (data.lockedBy === parseInt(document.getElementById("PassengerId").value)) {
		return;
}

const button = document.querySelector(`button[data-seat-id='${data.seatId}']`);

	if (button && button.classList.contains('btn-selected')) {
		alert("Това място току-що беше избрано от друг потребител.");
		button.classList.remove('btn-selected');
		document.getElementById('SeatId').value = "";
	}

	button.disabled = true;
	button.classList.remove('btn-available');
	button.classList.add('btn-locked');
});

document.addEventListener("DOMContentLoaded", function () {
	const seatButtons = document.querySelectorAll('button[data-seat-id]');
	const hiddenInput = document.getElementById('SeatId');

	const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

	let selectedButton = null;

	if (seatButtons.length === 0) return;

	seatButtons.forEach(btn => {
		btn.addEventListener('click', async function (e) {
			e.preventDefault();

			if (this.disabled || this.classList.contains('btn-locked') || this.classList.contains('btn-taken')) {
				console.warn("Мястото вече е заето или заключено");
				return;
			} 

			const seatId = this.dataset.seatId;
			const scheduleId = document.getElementById('ScheduleId').value;

			const passengerIdInput = document.getElementById('PassengerId');
			const passengerId = passengerIdInput ? passengerIdInput.value : '';

			if (!passengerId) {
				alert("Моля, първо изберете пътник от списъка.");
				return;
			}

			const sendSeatRequest = async (url, sId, stId, pId) => {
				const formData = new FormData();
				formData.append('scheduleId', sId);
				formData.append('seatId', stId);
				formData.append('passengerId', pId);

				if (token) formData.append('__RequestVerificationToken', token);

				const connectionId = (connection && connection.state === "Connected") ? connection.connectionId : "";

				return fetch(url, {
					method: 'POST',
					body: formData,
					headers: {
						'X-SignalR-ConnectionId': connectionId
					}
				});
			};

			if (this.classList.contains('btn-selected')) {
				this.classList.remove('btn-selected');
				this.classList.add('btn-available');
				selectedButton = null;

				if (hiddenInput) hiddenInput.value = "";

				await sendSeatRequest('/Reservations/UnlockSeat', scheduleId, seatId, passengerId);
				return;
			}

			if (selectedButton) {
				const oldSeatId = selectedButton.dataset.seatId;
				selectedButton.classList.remove('btn-selected');
				selectedButton.classList.add('btn-available');

				await sendSeatRequest('/Reservations/UnlockSeat', scheduleId, oldSeatId, passengerId);
			}

			this.classList.remove('btn-available');
			this.classList.add('btn-selected');
			selectedButton = this;

			if (hiddenInput) {
				hiddenInput.value = seatId;
				console.log("Selected seat set to: ", seatId);
			}

			try {
				const response = await sendSeatRequest('/Reservations/LockSeat', scheduleId, seatId, passengerId);

				if (!response.ok) throw new Error("Server error");

				const data = await response.json();

				if (!data.success) {
					alert(data.message || "Мястото вече е заето.");
					this.classList.remove('btn-selected');
					this.classList.add('btn-locked');
					selectedButton = null;
					if (hiddenInput) hiddenInput.value = "";
				}
			} catch (err) {
				console.error("Network error:", err);
			}
		});
	});
});
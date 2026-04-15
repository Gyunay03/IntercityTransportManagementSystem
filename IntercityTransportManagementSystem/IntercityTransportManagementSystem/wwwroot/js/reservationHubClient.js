const signalR = window.signalR;

if (!signalR) {
    throw new Error("SignalR library is missing! Check SeatMap.cshtml script tags.");
}

export const connection = new window.signalR.HubConnectionBuilder()
    .withUrl("/reservationHub")
    .withAutomaticReconnect()
    .build();

connection.on("SeatLocked", function (data) {
    const button = document.querySelector(`button[data-seat-id='${data.seatId}']`);

    if (button) {
        if (button.classList.contains('btn-selected')) {
            alert("Това място току-що беше избрано от друг потребител.");
            button.classList.remove('btn-selected');
            const hiddenInput = document.getElementById('SeatId');
            if (hiddenInput) hiddenInput.value = "";
        }

        button.disabled = true;
        button.classList.remove('btn-available');
        button.classList.add('btn-locked');
    }
});

connection.on("SeatUnlocked", function (data) {
    const button = document.querySelector(`button[data-seat-id='${data.seatId}']`);
    if (button) {
        button.disabled = false;
        button.classList.remove('btn-locked');
        button.classList.add('btn-available');
    }
});

connection.on("SeatReserved", function (data) {
    const button = document.querySelector(`button[data-seat-id='${data.seatId}']`);

    if (button) {
        button.disabled = true;
        button.classList.remove('btn-available', 'btn-selected', 'btn-locked');
        button.classList.add('btn-taken');
    }
});

async function start() {
    try {
        await connection.start()
        console.log("Connected to SignalR.");
    } catch (err) {
        console.log("SignalR Connection Error: ", err);
        setTimeout(start, 5000);
    }
}

start();
(function () {
    const display = document.getElementById('timerDisplay');
    const elapsedMsInput = document.getElementById('elapsedMs');
    const isFinishedInput = document.getElementById('isFinished');
    const serialState = document.getElementById('serialState');
    const submitRunBtn = document.getElementById('submitRunBtn');

    function formatTime(ms) {
        const totalSeconds = Math.floor(ms / 1000);
        const minutes = Math.floor(totalSeconds / 60);
        const seconds = totalSeconds % 60;
        const hundredths = Math.floor((ms % 1000) / 10);
        return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}.${String(hundredths).padStart(2, '0')}`;
    }

    async function refreshStopwatch() {
        try {
            const response = await fetch('/Home/StopwatchState', { cache: 'no-store' });
            if (!response.ok) return;

            const data = await response.json();
            const ms = Number(data.elapsedMs ?? 0);

            display.textContent = formatTime(ms);
            elapsedMsInput.value = ms;
            isFinishedInput.value = String(Boolean(data.isFinished));

            serialState.textContent = data.isRunning
                ? 'Running (Serial: Game Started!)'
                : `Idle${data.latestLine ? ` • Last: ${data.latestLine}` : ''}`;

            submitRunBtn.disabled = data.isRunning || ms <= 0;
        } catch {
            serialState.textContent = 'Serial unavailable';
            submitRunBtn.disabled = true;
        }
    }

    setInterval(refreshStopwatch, 200);
    refreshStopwatch();
})();
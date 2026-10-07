(() => {
    const form = document.getElementById("experiment-quiz-form");

    if (!form || !form.dataset.attemptToken) {
        return;
    }

    const button = document.getElementById("quiz-submit");
    const oldTarget = document.getElementById("old-submit-target");
    const error = document.getElementById("quiz-client-error");
    const radios = [...form.querySelectorAll('input[type="radio"]')];
    const questionNames = [...new Set(radios.map(input => input.name))];
    const storageKey = `experiment-metrics:${form.dataset.attemptToken}`;

    let state = {
        allQuestionsAnsweredAtUtc: null,
        oldSubmitLocationClickCount: 0,
        incompleteSubmitCount: 0
    };

    try {
        const saved = JSON.parse(sessionStorage.getItem(storageKey));

        if (saved && typeof saved === "object") {
            state.allQuestionsAnsweredAtUtc =
                typeof saved.allQuestionsAnsweredAtUtc === "string"
                    ? saved.allQuestionsAnsweredAtUtc
                    : null;

            for (const name of [
                "oldSubmitLocationClickCount",
                "incompleteSubmitCount"
            ]) {
                if (Number.isSafeInteger(saved[name]) && saved[name] >= 0) {
                    state[name] = saved[name];
                }
            }
        }
    } catch {
        // Vẫn thu thập trong trang nếu trình duyệt không cho lưu.
    }

    function persist() {
        try {
            sessionStorage.setItem(storageKey, JSON.stringify(state));
        } catch {
            // Không làm gián đoạn việc học.
        }
    }

    function answeredAll() {
        return questionNames.length > 0 &&
            questionNames.every(name =>
                radios.some(input => input.name === name && input.checked));
    }

    function markAnsweredAll() {
        if (answeredAll() && !state.allQuestionsAnsweredAtUtc) {
            state.allQuestionsAnsweredAtUtc = new Date().toISOString();
            persist();
        }
    }

    async function saveMetrics() {
        const body = new URLSearchParams();

        body.set("attemptToken", form.dataset.attemptToken);
        body.set(
            "oldSubmitLocationClickCount",
            String(state.oldSubmitLocationClickCount));
        body.set(
            "incompleteSubmitCount",
            String(state.incompleteSubmitCount));

        if (state.allQuestionsAnsweredAtUtc) {
            body.set(
                "allQuestionsAnsweredAtUtc",
                state.allQuestionsAnsweredAtUtc);
        }

        const antiForgery = form.querySelector(
            'input[name="__RequestVerificationToken"]');

        if (antiForgery) {
            body.set("__RequestVerificationToken", antiForgery.value);
        }

        const response = await fetch(form.dataset.metricsUrl, {
            method: "POST",
            credentials: "same-origin",
            body,
            keepalive: true
        });

        if (!response.ok) {
            throw new Error("Không lưu được dữ liệu thao tác.");
        }
    }

    function saveInBackground() {
        saveMetrics().catch(() => {
            // Sẽ gửi lại toàn bộ bộ đếm khi nộp bài.
        });
    }

    form.addEventListener("change", () => {
        markAnsweredAll();
        saveInBackground();
    });

    // Tự kiểm tra để mỗi lần nộp thiếu chỉ được đếm một lần.
    form.noValidate = true;

    let submitting = false;

    form.addEventListener("submit", async event => {
        event.preventDefault();

        if (submitting) {
            return;
        }

        if (!form.checkValidity()) {
            state.incompleteSubmitCount++;
            persist();
            saveInBackground();
            form.reportValidity();
            return;
        }

        markAnsweredAll();
        submitting = true;
        button.disabled = true;
        error.hidden = true;

        try {
            // Lưu dữ liệu trước khi gửi form chấm bài.
            await saveMetrics();

            HTMLFormElement.prototype.submit.call(form);
        } catch {
            submitting = false;
            button.disabled = false;
            error.textContent =
                "Chưa lưu được thao tác. Hãy kiểm tra kết nối và nhấn nộp lại.";
            error.hidden = false;
        }
    });

    if (oldTarget && form.dataset.moveSubmit === "true") {
        function matchButtonSize() {
            const bounds = button.getBoundingClientRect();
            oldTarget.style.width = `${bounds.width}px`;
            oldTarget.style.height = `${bounds.height}px`;
        }

        matchButtonSize();
        window.addEventListener("resize", matchButtonSize);

        oldTarget.addEventListener("click", () => {
            state.oldSubmitLocationClickCount++;
            persist();
            saveInBackground();
        });
    }
})();
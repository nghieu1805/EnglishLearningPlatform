document.addEventListener('click', event => {
 const button = event.target.closest('.speak');
 if (!button) return;
 if (!('speechSynthesis' in window)) { alert('Trình duyệt này chưa hỗ trợ đọc bằng giọng máy.'); return; }
 const voices = speechSynthesis.getVoices();
 const voice = voices.find(v => v.lang.toLowerCase() === button.dataset.lang.toLowerCase());
 if (!voice) { alert('Máy chưa có giọng ' + button.dataset.lang + '. Hãy cài giọng tương ứng hoặc dùng bản ghi âm khi được bổ sung.'); return; }
 const utterance = new SpeechSynthesisUtterance(button.dataset.word);
 utterance.lang = button.dataset.lang; utterance.voice = voice; utterance.rate = 0.85;
 speechSynthesis.cancel(); speechSynthesis.speak(utterance);
});
document.querySelectorAll('form[data-confirm]').forEach(form => form.addEventListener('submit', event => {
 if (!confirm(form.dataset.confirm)) event.preventDefault();
}));

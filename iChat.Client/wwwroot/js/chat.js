window.scrollToMessage = function (messageId) {
    const element = document.getElementById(`message-${messageId}`);
    if (element) {
        element.scrollIntoView({ behavior: "smooth", block: "center" });
    }
};

window.scrollToBottom = function (container) {
    if (container instanceof HTMLElement) {
        container.scrollTop = container.scrollHeight;
    }
};
window.observeScrollTop = function (element, dotnetHelper) {
    element.addEventListener('scroll', function () {
        if (element.scrollTop < 150) {
            dotnetHelper.invokeMethodAsync('OnNearTopScroll');
        }
    });
};
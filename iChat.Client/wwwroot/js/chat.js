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
window.getTopVisibleMessageId = function (containerSelector, messageSelectorPrefix) {
    const container = document.querySelector(containerSelector);
    if (!container) return null;

    const messages = container.querySelectorAll(`[id^="${messageSelectorPrefix}"]`);
    const containerTop = container.getBoundingClientRect().top;

    let closestMessageId = null;
    let smallestDistance = Number.POSITIVE_INFINITY;

    messages.forEach(msg => {
        const rect = msg.getBoundingClientRect();
        const distance = Math.abs(rect.top - containerTop);
        if (distance < smallestDistance) {
            smallestDistance = distance;
            closestMessageId = msg.id.replace(messageSelectorPrefix, "");
        }
    });

    return closestMessageId;
}

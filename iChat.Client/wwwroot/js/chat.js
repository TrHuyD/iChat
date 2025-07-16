window.scrollToMessage = function (messageId) {
    const element = document.getElementById(`message-${messageId}`);
    if (element) {
        element.scrollIntoView({ behavior: "smooth", block: "center" });
    }
};
window.scrollToMessageAuto = function (messageId) {
    const element = document.getElementById(`message-${messageId}`);
    if (element) {
        element.scrollIntoView({ behavior: "auto", block: "center" });
    }
};

window.scrollToBottom = function (container) {
    if (container instanceof HTMLElement) {
        container.scrollTop = container.scrollHeight - container.clientHeight;
    }
};
window.observeScrollTop = function (element, dotnetHelper) {
    element.addEventListener('scroll', function () {
        if (element.scrollTop < 150) {
            dotnetHelper.invokeMethodAsync('OnNearTopScroll');
        }
    });
};
window.captureScrollAnchor = function (container) {
    return {
        scrollTop: container.scrollTop,
        scrollHeight: container.scrollHeight
    };
};
window.restoreScrollAfterPrepend = function (container, previous) {
    const newScrollHeight = container.scrollHeight;
    container.scrollTop = newScrollHeight - (previous.scrollHeight - previous.scrollTop);
};
window.isScrollAtBottom = (element) => {
    if (!element) return false;
    const threshold = 50; 
    const scrollTop = element.scrollTop;
    const scrollHeight = element.scrollHeight;
    const clientHeight = element.clientHeight;
    return (scrollTop + clientHeight) >= (scrollHeight - threshold);
};
window.isScrollAtTop = (element) => {
    if (!element) return false;
    const threshold = 500;
    return element.scrollTop <= threshold;
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
window.triggerInputFileClick = function (element) {
    element.click();
};
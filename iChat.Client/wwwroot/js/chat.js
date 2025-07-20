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
//window.restoreScrollAfterPrepend = function (container, previous) {
//    const newScrollHeight = container.scrollHeight;
//    container.scrollTop = newScrollHeight - (previous.scrollHeight - previous.scrollTop);
//};
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
window.requestAnimationFrameThen = function (dotNetRef, methodName) {
    requestAnimationFrame(() => {
        dotNetRef.invokeMethodAsync(methodName);
    });
};
window.restoreScrollAfterPrepend = function (container, snapshot) {
    container.offsetHeight;


    const newScrollTop = container.scrollHeight - snapshot.scrollHeight + snapshot.scrollTop;
    container.scrollTop = Math.max(0, newScrollTop);
window.getCaretPosition = function (el) {
    return el.selectionStart;
};

window.getCaretCoordinates = function (element) {
    if (!element) return null;

    const selectionStart = element.selectionStart;
    const rect = element.getBoundingClientRect();
    const style = getComputedStyle(element);
    const lineHeight = parseFloat(style.lineHeight || "20");
    const approxCharWidth = parseFloat(style.fontSize || "14") * 0.6;

    const x = rect.left + (selectionStart * approxCharWidth);
    const y = rect.top + lineHeight;

    return { x, y };
};
window.setCaretPosition = (el, pos) => {
    el.setSelectionRange(pos, pos);
    el.focus();
};
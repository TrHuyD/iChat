//scrollhelpers.js
window.scrollhelpers = {
    getScrollMetrics: function (element) {
        return {
            scrollTop: element.scrollTop,
            scrollHeight: element.scrollHeight,
            clientHeight: element.clientHeight
        };
    },

    scrollToBottom: function (element) {
        if (element) {
            element.scrollTop = element.scrollHeight;
        }
    },

    restoreScrollPosition: function (element, prevScrollHeight, prevScrollTop) {
        if (element) {
            const scrollHeightDiff = element.scrollHeight - prevScrollHeight;
            element.scrollTop = prevScrollTop + scrollHeightDiff;
        }
    }
};

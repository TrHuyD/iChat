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
};
window.mentionHelper = {
    getCursorPos: function (input) {
        return input.selectionStart;
    },
    suppressNavigationKeys: function (e) {
        const keysToSuppress = ["ArrowUp", "ArrowDown", "Enter"];
        if (keysToSuppress.includes(e.key)) {
            e.preventDefault();
        }
    },
    getCursorCoordinates: function (input) {
        const selectionStart = input.selectionStart;

        // Create a div that mimics the input
        const div = document.createElement("div");
        const computed = getComputedStyle(input);

        for (const prop of computed) {
            div.style[prop] = computed[prop];
        }

        // Required adjustments for accuracy
        div.style.position = "absolute";
        div.style.visibility = "hidden";
        div.style.whiteSpace = "pre-wrap";
        div.style.wordWrap = "break-word";
        div.style.overflow = "auto"; // mimic scrolling
        div.style.height = input.offsetHeight + "px";
        div.style.width = input.offsetWidth + "px";
        div.style.padding = computed.padding; // important for textarea
        div.style.border = computed.border;   // important for textarea

        // Set the text up to the cursor
        const beforeText = input.value.substring(0, selectionStart);
        const afterText = input.value.substring(selectionStart);

        // Add a span at the cursor point
        const span = document.createElement("span");
        span.textContent = "\u200b"; // Zero-width space

        div.textContent = beforeText;
        div.appendChild(span);
        div.append(afterText);

        // Append to body to calculate position
        document.body.appendChild(div);

        const spanRect = span.getBoundingClientRect();
        const result = {
            top: spanRect.top + window.scrollY,
            left: spanRect.left + window.scrollX
        };

        document.body.removeChild(div);
        return result;
    }
};
window.getEditorPlainText = (el) => {
    return el.innerText;
};

window.setEditorHtml = (el, html) => {
    el.innerHTML = html;
};

// Utility: insert a DOM node at current selection
function insertNodeAtCaret(el, node) {
    var sel = window.getSelection();
    if (!sel || sel.rangeCount === 0) {
        el.appendChild(node);
        return;
    }
    var range = sel.getRangeAt(0);
    range.deleteContents();
    range.insertNode(node);

    // Move caret after the inserted node
    range.setStartAfter(node);
    range.collapse(true);
    sel.removeAllRanges();
    sel.addRange(range);
}

window.insertEmojiText = (el, emoji) => {
    var span = document.createElement("span");
    span.textContent = emoji;
    insertNodeAtCaret(el, span);
};

window.insertEmojiImage = (el, url) => {
    var img = document.createElement("img");
    img.src = url;
    img.className = "emoji-icon";
    img.style.width = "20px";
    img.style.height = "20px";
    img.style.verticalAlign = "middle";
    insertNodeAtCaret(el, img);
};

window.insertText = (el, text) => {
    var textNode = document.createTextNode(text);
    insertNodeAtCaret(el, textNode);
};
window.replaceEmoji = (el, triggerWord, isCustom, value) => {
    const sel = window.getSelection();
    if (!sel || sel.rangeCount === 0) return;

    const range = sel.getRangeAt(0);

    // Find the :word: text node backwards from caret
    let container = range.startContainer;
    let textNode = (container.nodeType === 3) ? container : null;

    if (!textNode) return;

    const text = textNode.textContent;
    const searchPattern = ":" + triggerWord;
    const idx = text.lastIndexOf(searchPattern);

    if (idx === -1) return;

    // Split into before, match, after
    const before = text.substring(0, idx);
    const after = text.substring(idx + searchPattern.length);

    // Replace the whole text node with before
    textNode.textContent = before;

    // Insert emoji node
    let node;
    if (isCustom) {
        node = document.createElement("img");
        node.src = value;
        node.className = "emoji-icon";
        node.style.width = "20px";
        node.style.height = "20px";
        node.style.verticalAlign = "middle";
    } else {
        node = document.createElement("span");
        node.textContent = value;
    }
    textNode.parentNode.insertBefore(node, textNode.nextSibling);

    // Add trailing space + remaining text
    const space = document.createTextNode(" ");
    const afterNode = document.createTextNode(after);
    textNode.parentNode.insertBefore(space, node.nextSibling);
    textNode.parentNode.insertBefore(afterNode, space.nextSibling);

    // Move caret after the space
    const newRange = document.createRange();
    newRange.setStartAfter(space);
    newRange.collapse(true);
    sel.removeAllRanges();
    sel.addRange(newRange);
};
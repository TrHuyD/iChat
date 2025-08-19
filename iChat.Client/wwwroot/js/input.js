window.getEditorPlainText = (el) => el.innerText;
window.setEditorHtml = (el, html) => { el.innerHTML = html; };

function isBoundary(ch) {
    return /\s|[.,;!?()"'[\]{}]/.test(ch);
}

function resolveTextNodeAtCaret(range) {
    let node = range.startContainer;
    let offset = range.startOffset;

    if (node.nodeType === Node.TEXT_NODE) {
        return { node, offset };
    }

    if (node.childNodes && node.childNodes.length) {
        if (offset > 0) {
            let probe = node.childNodes[offset - 1];
            while (probe && probe.nodeType !== Node.TEXT_NODE && probe.lastChild) {
                probe = probe.lastChild;
            }
            if (probe && probe.nodeType === Node.TEXT_NODE) {
                return { node: probe, offset: probe.textContent.length };
            }
        }
        if (offset < node.childNodes.length) {
            let probe = node.childNodes[offset];
            while (probe && probe.nodeType !== Node.TEXT_NODE && probe.firstChild) {
                probe = probe.firstChild;
            }
            if (probe && probe.nodeType === Node.TEXT_NODE) {
                return { node: probe, offset: 0 };
            }
        }
    }

    return { node: null, offset: 0 };
}

window.getEmojiTokenAtCaret = (el) => {
    const sel = window.getSelection();
    if (!sel || sel.rangeCount === 0) return null;

    const caretRange = sel.getRangeAt(0);
    const { node, offset } = resolveTextNodeAtCaret(caretRange);
    if (!node) return null;

    const text = node.textContent || "";
    let left = offset - 1;
    while (left >= 0 && !isBoundary(text[left])) left--;
    const start = left + 1;

    if (text[start] !== ':') return null;

    const tokenBeforeCaret = text.slice(start, offset);
    const hasClosing = tokenBeforeCaret.endsWith(':');
    const word = tokenBeforeCaret.slice(1, hasClosing ? -1 : undefined);

    if (!word) return null;
    return { word, hasClosing };
};

function insertEmojiForRange(range, isCustom, value) {
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

    range.deleteContents();
    range.insertNode(node);

    const space = document.createTextNode(" ");
    node.parentNode.insertBefore(space, node.nextSibling);

    const sel = window.getSelection();
    const after = document.createRange();
    after.setStartAfter(space);
    after.collapse(true);
    sel.removeAllRanges();
    sel.addRange(after);
}

window.replaceEmojiAtCaret = (el, triggerWord, isCustom, value) => {
    const sel = window.getSelection();
    if (!sel || sel.rangeCount === 0) return;

    const caretRange = sel.getRangeAt(0);
    const { node, offset } = resolveTextNodeAtCaret(caretRange);
    if (!node) return;

    const text = node.textContent || "";
    let left = offset - 1;
    while (left >= 0 && !isBoundary(text[left])) left--;
    const start = left + 1;

    if (text[start] !== ':') return;

    let right = offset;
    while (right < text.length && !isBoundary(text[right])) right++;

    const tokenFull = text.slice(start, right);
    const withClosing = `:${triggerWord}:`;
    const withoutClosing = `:${triggerWord}`;

    const r = document.createRange();
    if (tokenFull === withClosing || tokenFull === withoutClosing) {
        r.setStart(node, start);
        r.setEnd(node, start + tokenFull.length);
        insertEmojiForRange(r, isCustom, value);
    }
};
window.fetchWithCredentials = async function (url, options) {
    options = options || {};
    options.credentials = "include"; // Required for sending HttpOnly cookies
    return fetch(url, options).then(async res => {
        return {
            ok: res.ok,
            status: res.status,
            json: await res.json()
        };
    });
};

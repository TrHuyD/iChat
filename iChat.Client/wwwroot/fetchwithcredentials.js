window.fetchwithcredentials = async function (url, options) {
    options = options || {};
    options.credentials = "include"; 
    return fetch(url, options).then(async res => {
        return {
            ok: res.ok,
            status: res.status,
            json: await res.json()
        };
    });
};
window.logout = async function () {
    const response = await window.fetchwithcredentials("https://localhost:6051/api/Auth/refreshtoken/logout", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        }
    });

    if (response.ok) {
        console.log(" Logged out successfully");
    } else {
        console.error(` Logout failed with status ${response.status}`, response);
    }

    return response;
};

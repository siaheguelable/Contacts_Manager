window.auth = {
  async postJson(url, obj) {
    const resp = await fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(obj),
      credentials: "same-origin",
    });
    const text = await resp.text();
    return { status: resp.status, text };
  },
};

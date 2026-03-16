// Use relative API paths to avoid same-origin issues.
// Replace absolute URLs like "https://localhost:7278/api/auth/dang-ky" with "/api/auth/dang-ky".

/**
 * Example register function using relative URL.
 * Call register({ name: '...', email: '...', password: '...' })
 */
async function register(payload) {
    const resp = await fetch('/api/auth/dang-ky', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
        credentials: 'include' // if your API uses cookies/auth
    });

    if (!resp.ok) {
        const text = await resp.text();
        throw new Error(text || 'Registration failed');
    }

    return resp.json();
}
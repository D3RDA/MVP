const TOKEN_KEY = 'planner_token'
const USER_KEY = 'planner_user'

export function getToken() {
  return localStorage.getItem(TOKEN_KEY)
}

export function setSession(authResponse) {
  localStorage.setItem(TOKEN_KEY, authResponse.token)
  localStorage.setItem(USER_KEY, JSON.stringify({
    userId: authResponse.userId,
    name: authResponse.name,
    email: authResponse.email
  }))
}

export function getUser() {
  const raw = localStorage.getItem(USER_KEY)
  return raw ? JSON.parse(raw) : null
}

export function clearSession() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(USER_KEY)
}

export function isLoggedIn() {
  return Boolean(getToken())
}

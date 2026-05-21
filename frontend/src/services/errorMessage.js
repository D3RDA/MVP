export function getErrorMessage(error, fallback = 'Ismeretlen hiba történt.') {
  if (error.response) {
    const data = error.response.data

    if (typeof data === 'string') return data

    if (data?.title && data?.errors) {
      const details = Object.values(data.errors).flat().join(' ')
      return `${data.title}: ${details}`
    }

    if (data?.message) return data.message
    if (data?.title) return data.title

    return `Szerverhiba: HTTP ${error.response.status}`
  }

  if (error.request) {
    return 'Nem sikerült elérni a backendet. Ellenőrizd, hogy fut-e az ASP.NET backend a http://localhost:5111 címen, és hogy nincs-e CORS/API URL probléma.'
  }

  return error.message || fallback
}

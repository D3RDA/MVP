import { reactive } from 'vue'

const state = reactive({ toasts: [] })
let nextId = 1

export function useToast() {
  function show(message, type = 'success') {
    const id = nextId++
    state.toasts.push({ id, message, type })
    window.setTimeout(() => remove(id), 3200)
  }

  function remove(id) {
    const index = state.toasts.findIndex((toast) => toast.id === id)
    if (index !== -1) state.toasts.splice(index, 1)
  }

  return { toasts: state.toasts, show, remove }
}

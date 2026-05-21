<script setup>
import { useToast } from '../composables/useToast'

const { toasts, remove } = useToast()
</script>

<template>
  <div class="toast-stack" aria-live="polite">
    <button
      v-for="toast in toasts"
      :key="toast.id"
      class="toast"
      :class="`toast-${toast.type}`"
      type="button"
      @click="remove(toast.id)"
    >
      {{ toast.message }}
    </button>
  </div>
</template>

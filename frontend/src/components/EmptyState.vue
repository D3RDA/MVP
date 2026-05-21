<script setup>
defineProps({
  title: { type: String, required: true },
  message: { type: String, required: true },
  actionText: { type: String, default: '' }
})

defineEmits(['action'])
</script>

<template>
  <article class="empty-state">
    <div class="empty-icon">＋</div>
    <h2>{{ title }}</h2>
    <p>{{ message }}</p>
    <button v-if="actionText" type="button" @click="$emit('action')">{{ actionText }}</button>
  </article>
</template>

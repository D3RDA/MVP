<script setup>
defineProps({ message: String })
</script>

<template>
  <div v-if="message" class="error-box">{{ message }}</div>
</template>

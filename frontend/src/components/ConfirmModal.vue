<script setup>
defineProps({
  show: { type: Boolean, default: false },
  title: { type: String, default: 'Megerősítés' },
  message: { type: String, default: 'Biztosan végrehajtod ezt a műveletet?' },
  confirmText: { type: String, default: 'Törlés' },
  cancelText: { type: String, default: 'Mégsem' }
})

defineEmits(['confirm', 'cancel'])
</script>

<template>
  <Teleport to="body">
    <div v-if="show" class="modal-backdrop" @click.self="$emit('cancel')">
      <section class="modal-card" role="dialog" aria-modal="true">
        <h2>{{ title }}</h2>
        <p>{{ message }}</p>
        <div class="modal-actions">
          <button type="button" class="secondary" @click="$emit('cancel')">{{ cancelText }}</button>
          <button type="button" class="danger" @click="$emit('confirm')">{{ confirmText }}</button>
        </div>
      </section>
    </div>
  </Teleport>
</template>

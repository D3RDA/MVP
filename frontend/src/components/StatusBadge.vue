<script setup>
import { labelFor } from '../utils/labels'

defineProps({ value: { type: String, required: true } })
</script>

<template>
  <span class="badge" :class="`badge-${value}`">{{ labelFor(value) }}</span>
</template>

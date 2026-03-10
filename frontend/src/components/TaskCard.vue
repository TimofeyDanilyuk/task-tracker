<template>
  <div
    class="task-card"
    :style="`--card-color: ${priorityColor}`"
    @click="$emit('click')"
  >
    <div class="card-top">
      <span class="priority-badge" :style="`background:${priorityColor}22;color:${priorityColor}`">
        {{ priorityLabel }}
      </span>
      <button class="delete-btn" @click.stop="$emit('delete', task.id)">✕</button>
    </div>

    <h3 class="task-title">{{ task.title }}</h3>
    <p v-if="task.description" class="task-desc">{{ task.description }}</p>

    <div class="card-footer">
      <span v-if="task.dueDate" class="due" :class="{ overdue: isOverdue }">
        📅 {{ formatDate(task.dueDate) }}
      </span>
      <div class="card-badges">
        <span v-if="task.subTasks?.length" class="badge">⊟ {{ task.subTasks.length }}</span>
        <span v-if="task.comments?.length" class="badge">◎ {{ task.comments.length }}</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({ task: Object, stages: Array })
defineEmits(['click', 'delete'])

const priorities = [
  { label: 'Критичный', color: '#f87171' },
  { label: 'Высокий',   color: '#fb923c' },
  { label: 'Средний',   color: '#facc15' },
  { label: 'Низкий',    color: '#34d399' },
  { label: 'Минимум',   color: '#6b7280' },
]

const priorityLabel = computed(() => priorities[props.task.priority - 1]?.label ?? '—')
const priorityColor = computed(() => priorities[props.task.priority - 1]?.color ?? '#6b7280')
const isOverdue     = computed(() => props.task.dueDate && new Date(props.task.dueDate) < new Date())

function formatDate(d) {
  return new Date(d).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' })
}
</script>

<style scoped>
.task-card {
  background: var(--surface2);
  border: 1px solid var(--border);
  border-left: 3px solid var(--card-color, var(--accent));
  border-radius: var(--radius);
  padding: 16px;
  cursor: grab;
  transition: all .22s;
  display: flex;
  flex-direction: column;
  gap: 8px;
  user-select: none;
}
.task-card:active { cursor: grabbing; }
.task-card:hover {
  border-color: var(--card-color, var(--accent));
  transform: translateY(-2px);
  box-shadow: 0 8px 28px rgba(0,0,0,.3);
}

.card-top { display: flex; align-items: center; justify-content: space-between; }
.priority-badge {
  font-size: 11px; font-weight: 600;
  padding: 3px 10px; border-radius: 99px; letter-spacing: .3px;
}
.delete-btn {
  background: none; border: none; color: var(--muted);
  cursor: pointer; font-size: 12px; padding: 2px 6px;
  border-radius: 6px; transition: all .2s;
}
.delete-btn:hover { color: var(--danger); background: rgba(248,113,113,.1); }

.task-title { font-size: 14px; font-weight: 600; line-height: 1.4; }
.task-desc {
  font-size: 12px; color: var(--muted); line-height: 1.5;
  overflow: hidden; display: -webkit-box;
  -webkit-line-clamp: 2; -webkit-box-orient: vertical;
}

.card-footer { display: flex; align-items: center; justify-content: space-between; margin-top: 4px; }
.due { font-size: 12px; color: var(--muted); }
.due.overdue { color: var(--danger); }
.card-badges { display: flex; gap: 6px; }
.badge {
  font-size: 11px; color: var(--muted);
  background: var(--surface); border: 1px solid var(--border);
  padding: 2px 8px; border-radius: 99px;
}
</style>
<template>
  <div class="calendar-view">
    <header class="calendar-header">
      <button class="btn btn-ghost back-btn" @click="router.push('/')">
        ← Назад
      </button>
      <h1>Календарь дедлайнов</h1>
      <div class="calendar-controls">
        <button class="btn btn-ghost" @click="prevMonth">←</button>
        <span class="current-month">{{ monthYear }}</span>
        <button class="btn btn-ghost" @click="nextMonth">→</button>
        <select v-model="selectedBoard" class="board-select">
          <option :value="null">Все задачи</option>
          <option v-for="board in boards" :key="board.id" :value="board.id">
            {{ board.name }}
          </option>
        </select>
      </div>
    </header>

    <div class="calendar-container">
      <div class="calendar-weekdays">
        <div v-for="day in weekdays" :key="day" class="weekday">{{ day }}</div>
      </div>
      <div class="calendar-days">
        <div
          v-for="day in days"
          :key="day.date"
          class="calendar-day"
          :class="{
            'other-month': !day.isCurrentMonth,
            'today': day.isToday,
            'has-deadline': day.hasDeadline
          }"
          @mouseenter="hoverDay = day.date"
          @mouseleave="hoverDay = null"
        >
          <div class="day-number">{{ day.day }}</div>
          <div v-if="day.tasks.length" class="day-tasks">
            <div
              v-for="task in day.tasks"
              :key="task.id"
              class="task-dot"
              :style="`background: ${getPriorityColor(task.priority)}`"
              :title="`${task.title} (${task.priorityLabel})`"
            ></div>
          </div>
          <div v-if="hoverDay === day.date && day.tasks.length" class="day-tooltip">
            <div v-for="task in day.tasks" :key="task.id" class="tooltip-task">
              <span class="tooltip-priority" :style="`background: ${getPriorityColor(task.priority)}`"></span>
              <span class="tooltip-title">{{ task.title }}</span>
              <span class="tooltip-board" v-if="task.boardName">({{ task.boardName }})</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="calendar-legend">
      <div class="legend-item">
        <div class="legend-dot" style="background: #f87171"></div>
        <span>Критичный</span>
      </div>
      <div class="legend-item">
        <div class="legend-dot" style="background: #fb923c"></div>
        <span>Высокий</span>
      </div>
      <div class="legend-item">
        <div class="legend-dot" style="background: #facc15"></div>
        <span>Средний</span>
      </div>
      <div class="legend-item">
        <div class="legend-dot" style="background: #34d399"></div>
        <span>Низкий</span>
      </div>
      <div class="legend-item">
        <div class="legend-dot" style="background: #6b7280"></div>
        <span>Минимум</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useBoardsStore } from '@/stores/boards'
import { useTasksStore } from '@/stores/tasks'
import api from '@/api'

const router = useRouter()
const boardsStore = useBoardsStore()
const tasksStore = useTasksStore()

const currentDate = ref(new Date())
const selectedBoard = ref(null)
const hoverDay = ref(null)
const tasks = ref([])
const boards = ref([])

const weekdays = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс']

const monthYear = computed(() => {
  return currentDate.value.toLocaleDateString('ru-RU', { month: 'long', year: 'numeric' })
})

const days = computed(() => {
  const year = currentDate.value.getFullYear()
  const month = currentDate.value.getMonth()
  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)
  const startDay = firstDay.getDay() === 0 ? 6 : firstDay.getDay() - 1 // Пн = 0
  const daysInMonth = lastDay.getDate()

  const daysArray = []
  const today = new Date()
  const todayStr = today.toISOString().split('T')[0]

  // Добавляем дни предыдущего месяца
  for (let i = 0; i < startDay; i++) {
    const date = new Date(year, month, -startDay + i + 1)
    daysArray.push({
      date: date.toISOString().split('T')[0],
      day: date.getDate(),
      isCurrentMonth: false,
      isToday: false,
      hasDeadline: false,
      tasks: []
    })
  }

  // Дни текущего месяца
  for (let i = 1; i <= daysInMonth; i++) {
    const date = new Date(year, month, i)
    const dateStr = date.toISOString().split('T')[0]
    const dayTasks = tasks.value.filter(t => t.dueDate && t.dueDate.split('T')[0] === dateStr)
    daysArray.push({
      date: dateStr,
      day: i,
      isCurrentMonth: true,
      isToday: dateStr === todayStr,
      hasDeadline: dayTasks.length > 0,
      tasks: dayTasks
    })
  }

  // Добавляем дни следующего месяца до 42 ячеек (6 недель)
  const totalCells = 42
  const remaining = totalCells - daysArray.length
  for (let i = 1; i <= remaining; i++) {
    const date = new Date(year, month + 1, i)
    daysArray.push({
      date: date.toISOString().split('T')[0],
      day: date.getDate(),
      isCurrentMonth: false,
      isToday: false,
      hasDeadline: false,
      tasks: []
    })
  }

  return daysArray
})

function prevMonth() {
  currentDate.value = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() - 1, 1)
}

function nextMonth() {
  currentDate.value = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() + 1, 1)
}

function getPriorityColor(priority) {
  const colors = ['#f87171', '#fb923c', '#facc15', '#34d399', '#6b7280']
  return colors[(priority ?? 3) - 1]
}

async function loadTasks() {
  try {
    let data
    if (selectedBoard.value) {
      const res = await api.get(`/boards/${selectedBoard.value}/tasks`)
      data = res.data
    } else {
      const res = await api.get('/tasks')
      data = res.data
    }
    // Фильтруем задачи с дедлайном
    tasks.value = data.filter(t => t.dueDate).map(t => ({
      ...t,
      priorityLabel: ['Критичный', 'Высокий', 'Средний', 'Низкий', 'Минимум'][(t.priority ?? 3) - 1],
      boardName: t.board?.name
    }))
  } catch (err) {
    console.error('Ошибка загрузки задач:', err)
  }
}

async function loadBoards() {
  await boardsStore.fetchAll()
  boards.value = boardsStore.boards
}

onMounted(async () => {
  await loadBoards()
  await loadTasks()
})

// При изменении выбранной доски перезагружаем задачи
watch(selectedBoard, loadTasks)
</script>

<style scoped>
.calendar-view {
  min-height: 100vh;
  background: var(--bg);
  padding: 20px 40px;
  max-width: 1200px;
  margin: 0 auto;
}

.calendar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 30px;
  flex-wrap: wrap;
  gap: 20px;
}

.calendar-header h1 {
  font-size: 28px;
  font-weight: 800;
  letter-spacing: -0.5px;
}

.calendar-controls {
  display: flex;
  align-items: center;
  gap: 12px;
}

.current-month {
  font-size: 18px;
  font-weight: 600;
  min-width: 200px;
  text-align: center;
}

.board-select {
  padding: 8px 12px;
  border-radius: 8px;
  border: 1px solid var(--border);
  background: var(--surface);
  color: var(--text);
  font-size: 14px;
}

.calendar-container {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 16px;
  overflow: hidden;
  margin-bottom: 30px;
}

.calendar-weekdays {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  background: var(--surface2);
  border-bottom: 1px solid var(--border);
}

.weekday {
  padding: 16px;
  text-align: center;
  font-weight: 600;
  color: var(--muted);
  font-size: 14px;
}

.calendar-days {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
}

.calendar-day {
  min-height: 120px;
  border-right: 1px solid var(--border);
  border-bottom: 1px solid var(--border);
  padding: 10px;
  position: relative;
  transition: background 0.2s;
}

.calendar-day:nth-child(7n) {
  border-right: none;
}

.calendar-day.other-month {
  background: var(--surface2);
  color: var(--muted);
}

.calendar-day.today {
  background: rgba(91, 127, 255, 0.1);
}

.calendar-day.has-deadline {
  background: rgba(248, 113, 113, 0.05);
}

.day-number {
  font-size: 16px;
  font-weight: 600;
  margin-bottom: 8px;
}

.day-tasks {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.task-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  cursor: help;
}

.day-tooltip {
  position: absolute;
  bottom: 100%;
  left: 50%;
  transform: translateX(-50%);
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 10px;
  z-index: 100;
  min-width: 200px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.tooltip-task {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
}

.tooltip-priority {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}

.tooltip-title {
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.tooltip-board {
  font-size: 11px;
  color: var(--muted);
}

.calendar-legend {
  display: flex;
  justify-content: center;
  gap: 20px;
  flex-wrap: wrap;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: var(--muted);
}

.legend-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
}

@media (max-width: 768px) {
  .calendar-view {
    padding: 16px;
  }

  .calendar-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .calendar-controls {
    width: 100%;
    justify-content: space-between;
  }

  .calendar-day {
    min-height: 80px;
    padding: 6px;
  }

  .day-number {
    font-size: 14px;
  }

  .calendar-legend {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }
}
</style>
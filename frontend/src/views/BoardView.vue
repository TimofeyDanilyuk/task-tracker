<template>
  <div class="board-layout">

    <!-- Оверлей для мобильного сайдбара -->
    <div class="sidebar-overlay" :class="{ active: sidebarOpen }" @click="sidebarOpen = false"></div>

    <aside class="sidebar" :class="{ open: sidebarOpen }">
      <div class="sidebar-logo">
        <span class="logo-icon">⬡</span>
        <span class="logo-text">Taskflow</span>
        <!-- Кнопка закрытия на мобильном -->
        <button class="sidebar-close" @click="sidebarOpen = false">✕</button>
      </div>
      <nav class="sidebar-nav">
        <a class="nav-item active">
          <span>⊞</span> Доска
        </a>
        <a class="nav-item" @click="showStagesModal = true; sidebarOpen = false">
          <span>◈</span> Этапы
        </a>
        <a class="nav-item" @click="router.push('/todo'); sidebarOpen = false">
          <span>✓</span> ToDo лист
        </a>
      </nav>
      <div class="user-badge">
        <span class="user-name">👤 {{ authStore.username }}</span>
        <button class="logout-btn" @click="logout">Выйти</button>
      </div>
      <div class="sidebar-bottom">
        <div class="stats-card">
          <div class="stat">
            <span class="stat-num">{{ tasks.length }}</span>
            <span class="stat-label">Задач</span>
          </div>
          <div class="stat-divider"></div>
          <div class="stat">
            <span class="stat-num">{{ stages.length }}</span>
            <span class="stat-label">Этапов</span>
          </div>
        </div>
      </div>
    </aside>

    <main class="board-main">
      <header class="board-header">
        <div class="board-header-left">
          <!-- Гамбургер для мобильного -->
          <button class="hamburger" @click="sidebarOpen = true">
            <span></span><span></span><span></span>
          </button>
          <div>
            <h1 class="board-title">Доска задач</h1>
            <p class="board-sub">{{ today }}</p>
          </div>
        </div>
        <button class="btn btn-primary" @click="showTaskModal = true">
          + Новая задача
        </button>
      </header>

      <div v-if="loading" class="empty-state">Загрузка...</div>

      <div v-else class="kanban-board">

        <!-- Без этапа -->
        <div class="kanban-lane">
          <div class="lane-header">
            <div class="lane-title-row">
              <span class="lane-dot" style="background:#6b7280"></span>
              <span class="lane-name">Без этапа</span>
              <span class="lane-count">{{ tasks.filter(t => !t.stageId).length }}</span>
            </div>
          </div>
          <draggable
            class="lane-cards"
            :list="tasks.filter(t => !t.stageId)"
            group="tasks"
            item-key="id"
            @change="onDragEnd($event, null)"
            @start="onDragStart"
            @end="onDragEndGlobal"
          >
            <template #item="{ element }">
              <TaskCard
                :task="element"
                :stages="stages"
                @click="openTask(element.id)"
                @delete="deleteTask"
              />
            </template>
            <template #footer>
              <div
                v-if="tasks.filter(t => !t.stageId).length === 0 && !isDragging"
                class="lane-empty"
              >
                Перетащите задачу сюда
              </div>
            </template>
          </draggable>
        </div>

        <!-- По этапам -->
        <div
          v-for="stage in stages"
          :key="stage.id"
          class="kanban-lane"
          :style="`--lane-color: ${stage.color}`"
        >
          <div class="lane-header">
            <div class="lane-title-row">
              <span class="lane-dot" :style="`background:${stage.color}`"></span>
              <span class="lane-name">{{ stage.name }}</span>
              <span class="lane-count">{{ tasks.filter(t => t.stageId === stage.id).length }}</span>
            </div>
            <div class="lane-line" :style="`background:${stage.color}`"></div>
          </div>
          <draggable
            class="lane-cards"
            :list="tasks.filter(t => t.stageId === stage.id)"
            group="tasks"
            item-key="id"
            @change="onDragEnd($event, stage.id)"
            @start="onDragStart"
            @end="onDragEndGlobal"
          >
            <template #item="{ element }">
              <TaskCard
                :task="element"
                :stages="stages"
                @click="openTask(element.id)"
                @delete="deleteTask"
              />
            </template>
            <template #footer>
              <div
                v-if="tasks.filter(t => t.stageId === stage.id).length === 0 && !isDragging"
                class="lane-empty"
              >
                Перетащите задачу сюда
              </div>
            </template>
          </draggable>
        </div>

        <!-- Совсем пусто -->
        <div v-if="stages.length === 0 && tasks.length === 0" class="board-empty">
          <div class="empty-icon">◎</div>
          <p>Создайте этапы и задачи чтобы начать</p>
          <div style="display:flex;gap:10px;justify-content:center;margin-top:8px">
            <button class="btn btn-ghost" @click="showStagesModal = true">+ Этап</button>
            <button class="btn btn-primary" @click="showTaskModal = true">+ Задача</button>
          </div>
        </div>

      </div>
    </main>

    <TaskModal
      v-if="showTaskModal"
      :stages="stages"
      @close="showTaskModal = false"
      @created="onTaskCreated"
    />
    <StagesModal
      v-if="showStagesModal"
      :stages="stages"
      @close="showStagesModal = false"
      @updated="stagesStore.fetch()"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useTasksStore } from '@/stores/tasks'
import { useStagesStore } from '@/stores/stages'
import { storeToRefs } from 'pinia'
import draggable from 'vuedraggable'
import TaskCard from '@/components/TaskCard.vue'
import TaskModal from '@/components/TaskModal.vue'
import StagesModal from '@/components/StagesModal.vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
function logout() {
  authStore.logout()
  router.push('/auth')
}
const router = useRouter()
const tasksStore = useTasksStore()
const stagesStore = useStagesStore()
const { tasks } = storeToRefs(tasksStore)
const { stages } = storeToRefs(stagesStore)

const loading = ref(true)
const showTaskModal = ref(false)
const showStagesModal = ref(false)
const isDragging = ref(false)
const sidebarOpen = ref(false)

const today = computed(() => new Date().toLocaleDateString('ru-RU', {
  weekday: 'long', day: 'numeric', month: 'long'
}))

onMounted(async () => {
  await Promise.all([tasksStore.fetch(false), stagesStore.fetch()])
  loading.value = false
})

function openTask(id) { router.push(`/task/${id}`) }

function onDragStart() { isDragging.value = true }
function onDragEndGlobal() { isDragging.value = false }

async function onDragEnd(evt, targetStageId) {
  const element = evt.added?.element
  if (!element) return
  if (element.stageId === targetStageId) return

  const task = tasks.value.find(t => t.id === element.id)
  if (task) task.stageId = targetStageId

  await tasksStore.changeStage(element.id, targetStageId)
}

async function deleteTask(id) {
  await tasksStore.remove(id)
  await tasksStore.fetch()
}

function onTaskCreated() {
  showTaskModal.value = false
  tasksStore.fetch()
}
</script>

<style scoped>
.board-layout { display: flex; min-height: 100vh; }

/* Sidebar */
.sidebar {
  width: 230px;
  background: var(--surface);
  border-right: 1px solid var(--border);
  display: flex;
  flex-direction: column;
  padding: 28px 16px;
  position: sticky;
  top: 0;
  height: 100vh;
  flex-shrink: 0;
  z-index: 100;
}
.sidebar-logo { display: flex; align-items: center; gap: 10px; padding: 0 8px; margin-bottom: 36px; }
.logo-icon { font-size: 24px; color: var(--accent); }
.logo-text { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 20px; letter-spacing: -0.5px; flex: 1; }
.sidebar-close { display: none; background: none; border: none; color: var(--muted); font-size: 18px; cursor: pointer; padding: 4px; }
.sidebar-nav { display: flex; flex-direction: column; gap: 4px; }
.nav-item {
  display: flex; align-items: center; gap: 10px;
  padding: 10px 12px; border-radius: 9px; cursor: pointer;
  font-size: 14px; color: var(--muted); transition: all .2s;
}
.nav-item:hover, .nav-item.active { background: var(--surface2); color: var(--text); }
.nav-item.active { color: var(--accent); }
.sidebar-bottom { margin-top: auto; }
.stats-card {
  background: var(--surface2); border: 1px solid var(--border);
  border-radius: 12px; padding: 16px;
  display: flex; align-items: center; justify-content: space-around;
}
.stat { text-align: center; }
.stat-num { display: block; font-family: 'Syne', sans-serif; font-size: 22px; font-weight: 700; color: var(--accent); }
.stat-label { font-size: 11px; color: var(--muted); text-transform: uppercase; letter-spacing: .5px; }
.stat-divider { width: 1px; height: 32px; background: var(--border); }

.user-badge {
  display: flex; align-items: center; justify-content: space-between;
  padding: 10px 12px; margin-top: 8px;
  background: var(--surface2); border-radius: 9px;
  border: 1px solid var(--border);
}
.user-name { font-size: 13px; color: var(--muted); }
.logout-btn {
  background: none; border: none; color: var(--muted);
  cursor: pointer; font-size: 12px; transition: color .2s;
}
.logout-btn:hover { color: var(--danger); }

/* Гамбургер */
.hamburger {
  display: none;
  flex-direction: column;
  gap: 5px;
  background: none;
  border: none;
  cursor: pointer;
  padding: 6px;
  border-radius: 8px;
  transition: background .2s;
}
.hamburger:hover { background: var(--surface2); }
.hamburger span {
  display: block;
  width: 22px;
  height: 2px;
  background: var(--text);
  border-radius: 99px;
  transition: all .2s;
}

/* Оверлей */
.sidebar-overlay {
  display: none;
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,.5);
  z-index: 99;
  opacity: 0;
  transition: opacity .25s;
}
.sidebar-overlay.active { opacity: 1; }

/* Main */
.board-main { flex: 1; padding: 40px 36px; overflow-y: auto; min-width: 0; }
.board-header {
  display: flex; align-items: flex-start; justify-content: space-between; margin-bottom: 36px;
}
.board-header-left { display: flex; align-items: center; gap: 14px; }
.board-title { font-size: 28px; font-weight: 800; letter-spacing: -0.5px; }
.board-sub { font-size: 13px; color: var(--muted); margin-top: 4px; text-transform: capitalize; }

/* Kanban */
.kanban-board { display: flex; flex-direction: column; gap: 8px; }
.kanban-lane {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 16px;
  overflow: hidden;
  transition: border-color .2s;
}
.kanban-lane:has(.lane-line) {
  border-top: 3px solid var(--lane-color, var(--border));
}
.lane-header { padding: 16px 20px 12px; }
.lane-title-row { display: flex; align-items: center; gap: 10px; }
.lane-dot { width: 10px; height: 10px; border-radius: 50%; flex-shrink: 0; }
.lane-name { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 15px; }
.lane-count {
  background: var(--surface2); border: 1px solid var(--border);
  border-radius: 99px; font-size: 11px; padding: 1px 8px;
  color: var(--muted); font-family: 'DM Sans', sans-serif;
}
.lane-line { height: 2px; margin-top: 12px; border-radius: 99px; opacity: .5; }
.lane-cards {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 12px;
  padding: 4px 20px 20px;
  min-height: 80px;
}
.lane-empty {
  grid-column: 1 / -1; text-align: center; padding: 24px;
  color: var(--muted); font-size: 13px;
  border: 1px dashed var(--border); border-radius: 10px; transition: border-color .2s;
}
.lane-cards:hover .lane-empty { border-color: var(--accent); }
.board-empty {
  text-align: center; padding: 80px 20px; color: var(--muted);
  display: flex; flex-direction: column; align-items: center; gap: 12px;
}
.empty-icon { font-size: 48px; opacity: .3; }
.empty-state { text-align: center; padding: 80px; color: var(--muted); }

/* Mobile */
@media (max-width: 768px) {
  .hamburger { display: flex; }
  .sidebar-close { display: block; }
  .sidebar-overlay { display: block; pointer-events: none; }
  .sidebar-overlay.active { pointer-events: all; }

  .sidebar {
    position: fixed;
    left: -260px;
    top: 0;
    height: 100vh;
    width: 240px;
    transition: left .25s ease;
    box-shadow: none;
  }
  .sidebar.open {
    left: 0;
    box-shadow: 4px 0 24px rgba(0,0,0,.4);
  }

  .board-main { padding: 20px 16px; }
  .board-header { margin-bottom: 20px; align-items: center; }
  .board-title { font-size: 20px; }
  .board-sub { font-size: 11px; }

  .lane-cards {
    grid-template-columns: 1fr;
    padding: 4px 12px 12px;
  }
}
</style>
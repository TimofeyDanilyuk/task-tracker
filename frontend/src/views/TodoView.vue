<template>
  <div class="board-layout">

    <!-- Оверлей для мобильного сайдбара -->
    <div class="sidebar-overlay" :class="{ active: sidebarOpen }" @click="sidebarOpen = false"></div>

    <aside class="sidebar" :class="{ open: sidebarOpen }">
      <div class="sidebar-logo">
        <span class="logo-icon">⬡</span>
        <span class="logo-text">Taskflow</span>
        <button class="sidebar-close" @click="sidebarOpen = false">✕</button>
      </div>
      <nav class="sidebar-nav">
        <a class="nav-item" @click="router.push('/'); sidebarOpen = false"><span>⊞</span> Доска</a>
        <a class="nav-item active"><span>✓</span> ToDo</a>
        <a class="nav-item" @click="router.push('/friends'); sidebarOpen = false">
          <span>👥</span> Друзья
          <span v-if="friendsStore.requestsCount > 0" class="nav-badge">{{ friendsStore.requestsCount }}</span>
        </a>
      </nav>
      <div class="user-badge">
        <span class="user-name">👤 {{ authStore.username }}</span>
        <button class="logout-btn" @click="logout">Выйти</button>
      </div>
      <div class="sidebar-bottom">
        <div class="stats-card">
          <div class="stat">
            <span class="stat-num">{{ activeTasks.length }}</span>
            <span class="stat-label">Активных</span>
          </div>
          <div class="stat-divider"></div>
          <div class="stat">
            <span class="stat-num">{{ doneTasks.length }}</span>
            <span class="stat-label">Готово</span>
          </div>
        </div>
      </div>
    </aside>

    <main class="board-main">
      <header class="board-header">
        <div class="board-header-left">
          <button class="hamburger" @click="sidebarOpen = true">
            <span></span><span></span><span></span>
          </button>
          <div>
            <h1 class="board-title">ToDo</h1>
            <p class="board-sub">{{ today }}</p>
          </div>
        </div>
        <button class="btn btn-primary" @click="showModal = true">+ Новая задача</button>
      </header>

      <div v-if="loading" class="empty-state">Загрузка...</div>

      <div v-else class="todo-layout">

        <!-- Активные -->
        <div v-if="activeTasks.length > 0">
          <div class="section-label">
            <span>В процессе</span>
            <span class="lane-count">{{ activeTasks.length }}</span>
          </div>
          <div class="todo-grid">
            <div
              v-for="task in activeTasks"
              :key="task.id"
              class="todo-card"
              :style="`--p-color: ${getPriorityColor(task.priority)}`"
            >
              <div class="tc-head">
                <div class="tc-badges">
                  <span class="priority-pip" :style="`background:${getPriorityColor(task.priority)}`"></span>
                  <span class="priority-label" :style="`color:${getPriorityColor(task.priority)}`">
                    {{ getPriorityLabel(task.priority) }}
                  </span>
                  <span
                    v-if="task.stage"
                    class="stage-pill"
                    :style="`background:${task.stage.color}20;color:${task.stage.color};border-color:${task.stage.color}50`"
                  >
                    {{ task.stage.name }}
                  </span>
                </div>
                <button class="delete-btn" @click="deleteTask(task.id)">✕</button>
              </div>

              <div class="tc-body">
                <h3 class="tc-title">{{ task.title }}</h3>
                <p v-if="task.description" class="tc-desc">{{ task.description }}</p>
              </div>

              <div class="tc-progress">
                <div class="progress-track">
                  <div
                    class="progress-fill"
                    :style="`width:${getProgress(task)}%;background:${getProgressColor(task)}`"
                  ></div>
                </div>
                <span class="progress-num">{{ getDoneCount(task) }}/{{ task.subTasks?.length }}</span>
              </div>

              <div class="tc-checklist">
                <div
                  v-for="sub in task.subTasks"
                  :key="sub.id"
                  class="check-item"
                  :class="{ checked: sub.isDone }"
                  @click="toggle(sub, task)"
                >
                  <div class="check-box" :class="{ checked: sub.isDone }">
                    <svg v-if="sub.isDone" viewBox="0 0 12 12" fill="none">
                      <path d="M2 6l3 3 5-5" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/>
                    </svg>
                  </div>
                  <span>{{ sub.title }}</span>
                </div>
              </div>

              <div v-if="task.dueDate" class="tc-footer">
                <span class="due-date" :class="{ overdue: isOverdue(task) }">
                  {{ isOverdue(task) ? '⚠' : '📅' }} {{ formatDate(task.dueDate) }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Завершённые -->
        <div v-if="doneTasks.length > 0">
          <div class="section-label muted">
            <span>Завершено</span>
            <span class="lane-count">{{ doneTasks.length }}</span>
          </div>
          <div class="todo-grid">
            <div
              v-for="task in doneTasks"
              :key="task.id"
              class="todo-card done-card"
            >
              <div class="tc-head">
                <h3 class="tc-title done-text">{{ task.title }}</h3>
                <button class="delete-btn" @click="deleteTask(task.id)">✕</button>
              </div>
              <div class="tc-checklist">
                <div
                  v-for="sub in task.subTasks"
                  :key="sub.id"
                  class="check-item checked"
                  @click="toggle(sub, task)"
                >
                  <div class="check-box checked">
                    <svg viewBox="0 0 12 12" fill="none">
                      <path d="M2 6l3 3 5-5" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/>
                    </svg>
                  </div>
                  <span>{{ sub.title }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Пусто -->
        <div v-if="allTasks.length === 0" class="empty-state" style="padding:80px 0">
          <div class="empty-icon">✓</div>
          <p>Задач нет — создай первую!</p>
          <button class="btn btn-primary" @click="showModal = true">+ Новая задача</button>
        </div>

      </div>
    </main>

    <!-- Модалка -->
    <div class="modal-overlay" v-if="showModal" @click.self="closeModal">
      <div class="modal">
        <div class="modal-header">
          <h2>Новая ToDo задача</h2>
          <button class="btn btn-ghost" @click="closeModal">✕</button>
        </div>

        <div class="form-group">
          <label>Название *</label>
          <input v-model="form.title" placeholder="Что нужно сделать?" />
        </div>
        <div class="form-group">
          <label>Описание</label>
          <textarea v-model="form.description" rows="2" placeholder="Подробности..."></textarea>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Приоритет</label>
            <select v-model="form.priority">
              <option v-for="(p, i) in priorities" :key="i" :value="i+1">{{ p }}</option>
            </select>
          </div>
          <div class="form-group">
            <label>Этап</label>
            <select v-model="form.stageId">
              <option :value="null">— Без этапа —</option>
              <option v-for="s in stages" :key="s.id" :value="s.id">{{ s.name }}</option>
            </select>
          </div>
        </div>
        <div class="form-group">
          <label>Дедлайн</label>
          <input type="date" v-model="form.dueDate" />
        </div>

        <div class="form-group">
          <label>Подзадачи * (минимум одна)</label>
          <div class="sub-inputs">
            <div v-for="(sub, i) in form.subTasks" :key="i" class="sub-input-row">
              <input v-model="sub.title" :placeholder="`Подзадача ${i + 1}`" @keyup.enter="addSub" />
              <button class="delete-btn" @click="form.subTasks.splice(i, 1)">✕</button>
            </div>
          </div>
          <button class="btn btn-ghost add-sub-btn" @click="addSub">+ Добавить подзадачу</button>
        </div>

        <div class="modal-actions">
          <button class="btn btn-ghost" @click="closeModal">Отмена</button>
          <button
            class="btn btn-primary"
            :disabled="!form.title.trim() || form.subTasks.filter(s => s.title.trim()).length === 0"
            @click="createTask"
          >
            Создать
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useStagesStore } from '@/stores/stages'
import { storeToRefs } from 'pinia'
import api from '@/api'
import { useAuthStore } from '@/stores/auth'
import { useFriendsStore } from '@/stores/friends'

const friendsStore = useFriendsStore()

const authStore = useAuthStore()
function logout() {
  authStore.logout()
  router.push('/auth')
}
const router = useRouter()
const stagesStore = useStagesStore()
const { stages } = storeToRefs(stagesStore)

const loading = ref(true)
const showModal = ref(false)
const allTasks = ref([])
const sidebarOpen = ref(false)

const priorities = ['Критичный', 'Высокий', 'Средний', 'Низкий', 'Минимум']
const priorityColors = ['#f87171', '#fb923c', '#facc15', '#34d399', '#6b7280']

const form = reactive({
  title: '', description: '', priority: 3,
  stageId: null, dueDate: null, subTasks: [{ title: '' }]
})

const today = computed(() => new Date().toLocaleDateString('ru-RU', {
  weekday: 'long', day: 'numeric', month: 'long'
}))

const activeTasks = computed(() =>
  allTasks.value.filter(t => !t.subTasks?.every(s => s.isDone))
)
const doneTasks = computed(() =>
  allTasks.value.filter(t => t.subTasks?.length > 0 && t.subTasks.every(s => s.isDone))
)

function getPriorityLabel(p) { return priorities[(p ?? 3) - 1] }
function getPriorityColor(p) { return priorityColors[(p ?? 3) - 1] }
function getDoneCount(t) { return t.subTasks?.filter(s => s.isDone).length ?? 0 }
function getProgress(t) {
  if (!t.subTasks?.length) return 0
  return Math.round(getDoneCount(t) / t.subTasks.length * 100)
}
function getProgressColor(t) {
  const p = getProgress(t)
  if (p === 100) return '#34d399'
  if (p >= 50) return '#facc15'
  return '#5b7fff'
}
function isOverdue(t) { return t.dueDate && new Date(t.dueDate) < new Date() }
function formatDate(d) {
  return new Date(d).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' })
}

async function load() {
  const { data } = await api.get('/tasks', { params: { isTodo: true } })
  const full = await Promise.all(data.map(t => api.get(`/tasks/${t.id}`).then(r => r.data)))
  allTasks.value = full
}

onMounted(async () => {
  await Promise.all([stagesStore.fetch(), load(), friendsStore.fetchRequestsCount()])
  loading.value = false
})

function addSub() { form.subTasks.push({ title: '' }) }

function closeModal() {
  form.title = ''; form.description = ''; form.priority = 3
  form.stageId = null; form.dueDate = null; form.subTasks = [{ title: '' }]
  showModal.value = false
}

async function createTask() {
  const { data: task } = await api.post('/tasks', {
    title: form.title, description: form.description,
    priority: form.priority, stageId: form.stageId,
    dueDate: form.dueDate ? new Date(form.dueDate).toISOString() : null,
    isTodo: true
  })
  await Promise.all(
    form.subTasks
      .filter(s => s.title.trim())
      .map(s => api.post('/tasks', { title: s.title, priority: form.priority, parentTaskId: task.id }))
  )
  closeModal()
  await load()
}

async function toggle(sub, task) {
  await api.patch(`/tasks/${sub.id}/done`)
  sub.isDone = !sub.isDone
}

async function deleteTask(id) {
  await api.delete(`/tasks/${id}`)
  allTasks.value = allTasks.value.filter(t => t.id !== id)
}
</script>

<style scoped>
.board-layout { display: flex; height: 100vh; overflow: hidden; }

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
.board-main { flex: 1; padding: 40px 36px; overflow-y: auto; min-width: 0; height: 100vh; }
.board-header {
  display: flex; align-items: flex-start; justify-content: space-between; margin-bottom: 36px;
}
.board-header-left { display: flex; align-items: center; gap: 14px; }
.board-title { font-size: 28px; font-weight: 800; letter-spacing: -0.5px; }
.board-sub { font-size: 13px; color: var(--muted); margin-top: 4px; text-transform: capitalize; }

.todo-layout { display: flex; flex-direction: column; gap: 36px; }

.section-label {
  display: flex; align-items: center; gap: 10px;
  font-family: 'Syne', sans-serif; font-weight: 700;
  font-size: 13px; text-transform: uppercase; letter-spacing: .6px;
  color: var(--text); margin-bottom: 16px;
}
.section-label.muted { color: var(--muted); }
.lane-count {
  background: var(--surface2); border: 1px solid var(--border);
  border-radius: 99px; font-size: 11px; padding: 1px 8px;
  color: var(--muted);
}

.todo-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 16px;
}

.todo-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-top: 3px solid var(--p-color, var(--accent));
  border-radius: var(--radius);
  padding: 18px;
  display: flex; flex-direction: column; gap: 14px;
  transition: all .2s;
}
.todo-card:hover {
  border-color: var(--p-color, var(--accent));
  box-shadow: 0 8px 28px rgba(0,0,0,.25);
  transform: translateY(-2px);
}
.done-card { border-top-color: var(--border) !important; opacity: .55; }

.tc-head { display: flex; align-items: center; justify-content: space-between; gap: 8px; }
.tc-badges { display: flex; align-items: center; gap: 6px; flex-wrap: wrap; }
.priority-pip { width: 7px; height: 7px; border-radius: 50%; flex-shrink: 0; }
.priority-label { font-size: 11px; font-weight: 600; letter-spacing: .3px; }
.stage-pill { font-size: 11px; font-weight: 500; padding: 2px 9px; border-radius: 99px; border: 1px solid; }

.tc-body { display: flex; flex-direction: column; gap: 4px; }
.tc-title { font-size: 15px; font-weight: 700; line-height: 1.3; }
.tc-desc { font-size: 12px; color: var(--muted); line-height: 1.5; }
.done-text { text-decoration: line-through; color: var(--muted); font-weight: 400; }

.tc-progress { display: flex; align-items: center; gap: 10px; }
.progress-track { flex: 1; height: 4px; background: var(--surface2); border-radius: 99px; overflow: hidden; }
.progress-fill { height: 100%; border-radius: 99px; transition: width .35s ease; }
.progress-num { font-size: 11px; color: var(--muted); white-space: nowrap; }

.tc-checklist { display: flex; flex-direction: column; gap: 6px; }
.check-item {
  display: flex; align-items: center; gap: 9px;
  padding: 5px 7px; border-radius: 7px;
  cursor: pointer; font-size: 13px; transition: background .15s; color: var(--text);
}
.check-item:hover { background: var(--surface2); }
.check-item.checked { color: var(--muted); }
.check-item.checked span { text-decoration: line-through; }
.check-box {
  width: 17px; height: 17px; border-radius: 5px; flex-shrink: 0;
  border: 1.5px solid var(--border);
  display: flex; align-items: center; justify-content: center;
  transition: all .2s; color: #000;
}
.check-box.checked { background: var(--success); border-color: var(--success); }
.check-box svg { width: 10px; height: 10px; }

.tc-footer { display: flex; align-items: center; }
.due-date { font-size: 11px; color: var(--muted); }
.due-date.overdue { color: var(--danger); font-weight: 600; }

.sub-inputs { display: flex; flex-direction: column; gap: 7px; margin-bottom: 8px; }
.sub-input-row { display: flex; gap: 8px; align-items: center; }
.sub-input-row input { flex: 1; }
.add-sub-btn { width: 100%; justify-content: center; }
.modal-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 24px; }
textarea { resize: vertical; min-height: 60px; }

.empty-state {
  display: flex; flex-direction: column; align-items: center;
  gap: 14px; color: var(--muted); text-align: center;
}
.empty-icon { font-size: 44px; opacity: .25; }
.delete-btn {
  background: none; border: none; color: var(--muted);
  cursor: pointer; font-size: 12px; padding: 2px 6px;
  border-radius: 6px; transition: all .2s; flex-shrink: 0;
}
.delete-btn:hover { color: var(--danger); background: rgba(248,113,113,.1); }

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
  }
  .sidebar.open {
    left: 0;
    box-shadow: 4px 0 24px rgba(0,0,0,.4);
  }

  .board-main { padding: 20px 16px; }
  .board-header { margin-bottom: 20px; align-items: center; }
  .board-title { font-size: 20px; }
  .board-sub { font-size: 11px; }

  .todo-grid { grid-template-columns: 1fr; }

  .board-main { padding: 20px 16px; height: 100dvh; }
  .board-header {
    position: sticky;
    top: 0;
    z-index: 10;
    background: var(--bg);
    padding-bottom: 16px;
    margin-bottom: 0;
  }
}

.nav-badge {
  margin-left: auto;
  background: var(--danger);
  color: #fff;
  font-size: 11px;
  font-weight: 700;
  padding: 1px 7px;
  border-radius: 99px;
  min-width: 18px;
  text-align: center;
}
</style>
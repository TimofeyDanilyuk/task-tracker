<template>
  <div class="task-view" v-if="task">
    <!-- Шапка -->
    <header class="tv-header">
      <button class="btn btn-ghost back-btn" @click="router.push('/')">
        ← Назад
      </button>
      <div class="tv-header-right">
        <select
          class="stage-switcher"
          :value="task.stageId"
          :style="currentStage ? `border-color:${currentStage.color};color:${currentStage.color}` : ''"
          @change="changeStage(+$event.target.value)"
        >
          <option :value="null">— Без этапа —</option>
          <option v-for="s in stages" :key="s.id" :value="s.id">{{ s.name }}</option>
        </select>
        <button class="btn btn-danger" @click="deleteTask">Удалить задачу.</button>
      </div>
    </header>

    <div class="tv-body">
      <!-- Левая колонка -->
      <div class="tv-main">
        <!-- Заголовок задачи -->
        <div class="tv-title-row">
          <span class="priority-badge lg" :style="`background:${priorityColor}22;color:${priorityColor}`">
            {{ priorityLabel }}
          </span>
          <h1 class="tv-title" v-if="!editingTitle" @click="editingTitle = true">
            {{ task.title }}
            <span class="edit-hint">✎</span>
          </h1>
          <input
            v-else
            class="tv-title-input"
            v-model="editForm.title"
            @blur="saveTask"
            @keyup.enter="saveTask"
            autofocus
          />
        </div>

        <!-- Описание -->
        <div class="section">
          <label>Описание</label>
          <textarea
            v-if="editingDesc"
            v-model="editForm.description"
            rows="4"
            @blur="saveTask"
            autofocus
          ></textarea>
          <p
            v-else
            class="desc-text"
            @click="editingDesc = true"
          >
            {{ task.description || 'Нажмите чтобы добавить описание...' }}
            <span class="edit-hint">✎</span>
          </p>
        </div>

        <!-- Подзадачи -->
        <div class="section">
          <div class="section-header">
            <h3>Подзадачи <span class="count-badge">{{ task.subTasks?.length || 0 }}</span></h3>
            <button class="btn btn-ghost sm" @click="showSubModal = true">+ Добавить</button>
          </div>

          <div class="subtasks-list">
            <div
              v-for="sub in task.subTasks"
              :key="sub.id"
              class="subtask-row"
            >
              <div class="subtask-info">
                <span class="subtask-priority-dot" :style="`background:${getPriorityColor(sub.priority)}`"></span>
                <div>
                  <p class="subtask-title">{{ sub.title }}</p>
                  <p class="subtask-date">{{ formatDate(sub.createdAt) }}</p>
                  <p v-if="sub.description" class="subtask-desc">{{ sub.description }}</p>
                </div>
              </div>
              <div class="subtask-actions">
                <button class="icon-btn" @click="openSubComments(sub)">
                  ◎ {{ sub.comments?.length || 0 }}
                </button>
                <button class="icon-btn danger" @click="deleteSubTask(sub.id)">✕</button>
              </div>
            </div>
            <div v-if="!task.subTasks?.length" class="empty-sub">
              Подзадач пока нет
            </div>
          </div>
        </div>

        <!-- Комментарии к задаче -->
        <div class="section">
          <div class="section-header">
            <h3>Комментарии <span class="count-badge">{{ comments.length }}</span></h3>
          </div>
          <div class="comments-list">
            <div v-for="c in comments" :key="c.id" class="comment-row">
              <div class="comment-avatar">{{ c.text[0].toUpperCase() }}</div>
              <div class="comment-body">
                <p class="comment-text">{{ c.text }}</p>
                <span class="comment-date">{{ formatDate(c.createdAt) }}</span>
              </div>
              <button class="icon-btn danger" @click="removeComment(c.id)">✕</button>
            </div>
            <div v-if="!comments.length" class="empty-sub">Комментариев пока нет</div>
          </div>
          <div class="comment-input-row">
            <input
              v-model="newComment"
              placeholder="Написать комментарий..."
              @keyup.enter="addComment"
            />
            <button class="btn btn-primary" @click="addComment">Отправить</button>
          </div>
        </div>

        <!-- Связанные задачи -->
        <div class="section">
          <div class="section-header">
            <h3>Связанные задачи <span class="count-badge">{{ linkedTasks.length }}</span></h3>
            <button class="btn btn-ghost sm" @click="openLinkModal">+ Связать</button>
          </div>

          <div class="linked-list">
            <div
              v-for="link in linkedTasks"
              :key="link.id"
              class="linked-row"
              @click="router.push(`/task/${link.task.id}`)"
            >
              <div class="linked-left">
                <span
                  class="priority-badge sm"
                  :style="`background:${getPriorityColor(link.task.priority)}22;color:${getPriorityColor(link.task.priority)}`"
                >
                  {{ getPriorityLabel(link.task.priority) }}
                </span>
                <span class="linked-id">#{{ link.task.id }}</span>
                <span class="linked-title">{{ link.task.title }}</span>
              </div>
              <div class="linked-right">
                <span
                  v-if="link.task.stage"
                  class="stage-pill"
                  :style="`background:${link.task.stage.color}22;color:${link.task.stage.color};border-color:${link.task.stage.color}44`"
                >
                  {{ link.task.stage.name }}
                </span>
                <button class="icon-btn danger" @click.stop="removeLink(link.task.id)">✕</button>
              </div>
            </div>
            <div v-if="linkedTasks.length === 0" class="empty-sub">Нет связанных задач</div>
          </div>
        </div>
      </div>

      <!-- Правая колонка — мет -->
      <aside class="tv-sidebar">
        <div class="meta-card">
          <h4>Детали</h4>
          <div class="meta-row">
            <span class="meta-label">Приоритет</span>
            <span class="priority-badge sm" :style="`background:${priorityColor}22;color:${priorityColor}`">
              {{ priorityLabel }}
            </span>
          </div>
          <div class="meta-row">
            <span class="meta-label">Создана</span>
            <span>{{ formatDate(task.createdAt) }}</span>
          </div>
          <div class="meta-row">
            <span class="meta-label">Дедлайн</span>
            <span :class="{ overdue: isOverdue }">
              {{ task.dueDate ? formatDate(task.dueDate) : '—' }}
            </span>
          </div>
          <div class="meta-row">
            <span class="meta-label">Этап</span>
            <span
              v-if="currentStage"
              class="stage-pill"
              :style="`background:${currentStage.color}22;color:${currentStage.color};border-color:${currentStage.color}44`"
            >
              {{ currentStage.name }}
            </span>
            <span v-else class="meta-muted">—</span>
          </div>

          <!-- Редактировать дедлайн -->
          <div class="meta-edit">
            <label>Изменить дедлайн</label>
            <input type="date" v-model="editForm.dueDate" @change="saveTask" />
          </div>

          <!-- Редактировать приоритет -->
          <div class="meta-edit">
            <label>Изменить приоритет</label>
            <select v-model="editForm.priority" @change="saveTask">
              <option v-for="(p, i) in priorities" :key="i" :value="i+1">{{ p }}</option>
            </select>
          </div>
        </div>
      </aside>
    </div>

    <!-- Модалка: добавить подзадачу -->
    <div class="modal-overlay" v-if="showSubModal" @click.self="showSubModal = false">
      <div class="modal">
        <div class="modal-header">
          <h2>Новая подзадача</h2>
          <button class="btn btn-ghost" @click="showSubModal = false">✕</button>
        </div>
        <div class="form-group">
          <label>Название *</label>
          <input v-model="subForm.title" placeholder="Название подзадачи..." />
        </div>
        <div class="form-group">
          <label>Описание</label>
          <textarea v-model="subForm.description" rows="2" placeholder="Подробности..."></textarea>
        </div>
        <div class="form-group">
          <label>Приоритет</label>
          <select v-model="subForm.priority">
            <option v-for="(p, i) in priorities" :key="i" :value="i+1">{{ p }}</option>
          </select>
        </div>
        <div class="modal-actions">
          <button class="btn btn-ghost" @click="showSubModal = false">Отмена</button>
          <button class="btn btn-primary" :disabled="!subForm.title" @click="addSubTask">
            Создать
          </button>
        </div>
      </div>
    </div>

    <!-- Модалка: комментарии к подзадаче -->
    <div class="modal-overlay" v-if="activeSubTask" @click.self="activeSubTask = null">
      <div class="modal">
        <div class="modal-header">
          <h2>Комментарии: {{ activeSubTask.title }}</h2>
          <button class="btn btn-ghost" @click="activeSubTask = null">✕</button>
        </div>
        <div class="comments-list" style="max-height:260px;overflow-y:auto;margin-bottom:16px">
          <div v-for="c in subComments" :key="c.id" class="comment-row">
            <div class="comment-avatar">{{ c.text[0].toUpperCase() }}</div>
            <div class="comment-body">
              <p class="comment-text">{{ c.text }}</p>
              <span class="comment-date">{{ formatDate(c.createdAt) }}</span>
            </div>
            <button class="icon-btn danger" @click="removeSubComment(c.id)">✕</button>
          </div>
          <div v-if="!subComments.length" class="empty-sub">Комментариев пока нет</div>
        </div>
        <div class="comment-input-row">
          <input v-model="newSubComment" placeholder="Написать комментарий..." @keyup.enter="addSubComment" />
          <button class="btn btn-primary" @click="addSubComment">Отправить</button>
        </div>
      </div>
    </div>

    <!-- Модалка: связать задачи -->
    <div class="modal-overlay" v-if="showLinkModal" @click.self="showLinkModal = false">
      <div class="modal modal-lg">
        <div class="modal-header">
          <h2>Связать задачи</h2>
          <button class="btn btn-ghost" @click="showLinkModal = false">✕</button>
        </div>

        <div class="link-tasks-list">
          <div v-if="!allTasks.length" class="empty-sub">Нет доступных задач</div>
          <div
            v-for="t in allTasks"
            :key="t.id"
            class="link-task-row"
            :class="{ selected: selectedLinks.includes(t.id) }"
            @click="toggleLink(t.id)"
          >
            <div class="checkbox" :class="{ checked: selectedLinks.includes(t.id) }">
              <svg v-if="selectedLinks.includes(t.id)" viewBox="0 0 12 12" fill="none">
                <path d="M2 6l3 3 5-5" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/>
              </svg>
            </div>
            <span class="linked-id">#{{ t.id }}</span>
            <span class="link-task-title">{{ t.title }}</span>
            <span
              v-if="t.stage"
              class="stage-pill"
              :style="`background:${t.stage.color}22;color:${t.stage.color};border-color:${t.stage.color}44`"
            >
              {{ t.stage.name }}
            </span>
          </div>
        </div>

        <div class="modal-actions">
          <button class="btn btn-ghost" @click="showLinkModal = false">Отмена</button>
          <button
            class="btn btn-primary"
            :disabled="selectedLinks.length === 0"
            @click="addLinks"
          >
            Связать ({{ selectedLinks.length }})
          </button>
        </div>
      </div>
    </div>
  </div>

  <div v-else class="loading-screen">Загрузка...</div>
</template>

<script setup>
import { ref, computed, onMounted, reactive } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTasksStore } from '@/stores/tasks'
import { useStagesStore } from '@/stores/stages'
import { useCommentsStore } from '@/stores/comments'
import { storeToRefs } from 'pinia'
import api from '@/api'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
function logout() {
  authStore.logout()
  router.push('/auth')
}
const route = useRoute()
const router = useRouter()
const tasksStore = useTasksStore()
const stagesStore = useStagesStore()
const commentsStore = useCommentsStore()
const { stages } = storeToRefs(stagesStore)
const { comments } = storeToRefs(commentsStore)

const task = ref(null)
const editingTitle = ref(false)
const editingDesc = ref(false)
const showSubModal = ref(false)
const newComment = ref('')
const activeSubTask = ref(null)
const subComments = ref([])
const newSubComment = ref('')

const priorities = ['Критичный', 'Высокий', 'Средний', 'Низкий', 'Минимум']
const priorityColors = ['#f87171','#fb923c','#facc15','#34d399','#6b7280']

const editForm = reactive({ title: '', description: '', priority: 3, dueDate: null })
const subForm = reactive({ title: '', description: '', priority: 3 })

const currentStage  = computed(() => stages.value.find(s => s.id === task.value?.stageId))
const priorityLabel = computed(() => priorities[(editForm.priority ?? 3) - 1])
const priorityColor = computed(() => priorityColors[(editForm.priority ?? 3) - 1])
const isOverdue     = computed(() => task.value?.dueDate && new Date(task.value.dueDate) < new Date())

// Связанные задачи
const linkedTasks = ref([])
const showLinkModal = ref(false)
const allTasks = ref([])
const selectedLinks = ref([])

async function loadLinks() {
  const { data } = await api.get(`/tasks/${route.params.id}/links`)
  linkedTasks.value = data
}

async function openLinkModal() {
  const { data } = await api.get('/tasks')
  // Фильтруем: убираем текущую задачу и уже связанные
  const linkedIds = linkedTasks.value.map(l => l.task.id)
  allTasks.value = data.filter(t => t.id !== task.value.id && !linkedIds.includes(t.id))
  selectedLinks.value = []
  showLinkModal.value = true
}

async function addLinks() {
  await Promise.all(
    selectedLinks.value.map(id => api.post(`/tasks/${task.value.id}/links`, { linkedTaskId: id }))
  )
  showLinkModal.value = false
  await loadLinks()
}

async function removeLink(linkedTaskId) {
  await api.delete(`/tasks/${task.value.id}/links/${linkedTaskId}`)
  await loadLinks()
}

function toggleLink(id) {
  const i = selectedLinks.value.indexOf(id)
  if (i === -1) selectedLinks.value.push(id)
  else selectedLinks.value.splice(i, 1)
}

function getPriorityColor(p) { return priorityColors[(p ?? 3) - 1] }
function getPriorityLabel(p) { return priorities[(p ?? 3) - 1] }

function formatDate(d) {
  if (!d) return '—'
  return new Date(d).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short', year: 'numeric' })
}

async function load() {
  await Promise.all([stagesStore.fetch(), tasksStore.fetchOne(route.params.id)])
  task.value = tasksStore.current
  editForm.title       = task.value.title
  editForm.description = task.value.description
  editForm.priority    = task.value.priority
  editForm.dueDate     = task.value.dueDate ? task.value.dueDate.split('T')[0] : null
  await commentsStore.fetch(task.value.id)
  await loadLinks()
}

onMounted(load)

async function saveTask() {
  editingTitle.value = false
  editingDesc.value  = false
  await tasksStore.update(task.value.id, {
    title:       editForm.title,
    description: editForm.description,
    priority:    editForm.priority,
    stageId:     task.value.stageId,
    dueDate:     editForm.dueDate ? new Date(editForm.dueDate).toISOString() : null
  })
  await load()
}

async function changeStage(stageId) {
  await tasksStore.changeStage(task.value.id, stageId)
  task.value.stageId = stageId
}

async function deleteTask() {
  if (!confirm('Удалить задачу?')) return
  await tasksStore.remove(task.value.id)
  router.push('/')
}

// Подзадачи
async function addSubTask() {
  if (!subForm.title.trim()) return
  await api.post('/tasks', {
    title:        subForm.title,
    description:  subForm.description,
    priority:     subForm.priority,
    parentTaskId: task.value.id
  })
  subForm.title = ''
  subForm.description = ''
  subForm.priority = 3
  showSubModal.value = false
  await load()
}

async function deleteSubTask(id) {
  await tasksStore.remove(id)
  await load()
}

// Комментарии к задаче
async function addComment() {
  if (!newComment.value.trim()) return
  await commentsStore.create({ text: newComment.value, taskItemId: task.value.id })
  newComment.value = ''
}

async function removeComment(id) {
  await commentsStore.remove(id)
}

// Комментарии к подзадаче
async function openSubComments(sub) {
  activeSubTask.value = sub
  const { data } = await api.get(`/comments/task/${sub.id}`)
  subComments.value = data
}

async function addSubComment() {
  if (!newSubComment.value.trim()) return
  await api.post('/comments', { text: newSubComment.value, taskItemId: activeSubTask.value.id })
  newSubComment.value = ''
  await openSubComments(activeSubTask.value)
}

async function removeSubComment(id) {
  await api.delete(`/comments/${id}`)
  await openSubComments(activeSubTask.value)
}
</script>

<style scoped>
.task-view { min-height: 100vh; background: var(--bg); }

.tv-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 40px;
  border-bottom: 1px solid var(--border);
  background: var(--surface);
  position: sticky;
  top: 0;
  z-index: 10;
}
.tv-header-right { display: flex; align-items: center; gap: 12px; }
.back-btn { font-size: 14px; }

.stage-switcher {
  padding: 8px 14px;
  border-radius: 99px;
  font-size: 13px;
  font-weight: 500;
  width: auto;
  border: 1px solid var(--border);
  transition: all .2s;
}

.tv-body {
  display: grid;
  grid-template-columns: 1fr 300px;
  gap: 32px;
  padding: 40px;
  max-width: 1100px;
  margin: 0 auto;
}

/* Заголовок */
.tv-title-row { display: flex; align-items: flex-start; gap: 14px; margin-bottom: 28px; flex-wrap: wrap; }
.tv-title {
  font-size: 26px;
  font-weight: 800;
  letter-spacing: -0.5px;
  cursor: pointer;
  line-height: 1.3;
  flex: 1;
}
.tv-title:hover .edit-hint { opacity: 1; }
.tv-title-input {
  font-family: 'Syne', sans-serif;
  font-size: 24px;
  font-weight: 700;
  flex: 1;
}
.edit-hint { font-size: 14px; color: var(--muted); opacity: 0; transition: opacity .2s; margin-left: 8px; }

/* Секции */
.section { margin-bottom: 36px; }
.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 14px;
}
.section-header h3 { font-size: 16px; font-weight: 700; display: flex; align-items: center; gap: 8px; }
.count-badge {
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 99px;
  font-size: 11px;
  padding: 2px 8px;
  font-family: 'DM Sans', sans-serif;
  color: var(--muted);
}
label { font-size: 13px; color: var(--muted); margin-bottom: 8px; display: block; }

.desc-text {
  color: var(--muted);
  font-size: 14px;
  cursor: pointer;
  padding: 12px;
  border-radius: 9px;
  border: 1px dashed var(--border);
  transition: border-color .2s;
  line-height: 1.7;
}
.desc-text:hover { border-color: var(--accent); color: var(--text); }
.desc-text:hover .edit-hint { opacity: 1; }

/* Подзадачи */
.subtasks-list { display: flex; flex-direction: column; gap: 8px; }
.subtask-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 12px 16px;
  transition: border-color .2s;
}
.subtask-row:hover { border-color: var(--accent); }
.subtask-info { display: flex; align-items: flex-start; gap: 10px; }
.subtask-priority-dot { width: 8px; height: 8px; border-radius: 50%; margin-top: 6px; flex-shrink: 0; }
.subtask-title { font-size: 14px; font-weight: 500; }
.subtask-date { font-size: 11px; color: var(--muted); margin-top: 2px; }
.subtask-desc { font-size: 12px; color: var(--muted); margin-top: 2px; }
.subtask-actions { display: flex; align-items: center; gap: 6px; }

/* Комментарии */
.comments-list { display: flex; flex-direction: column; gap: 10px; margin-bottom: 14px; }
.comment-row {
  display: flex;
  gap: 12px;
  align-items: flex-start;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 12px 14px;
}
.comment-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: var(--accent);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 13px;
  font-weight: 700;
  flex-shrink: 0;
}
.comment-body { flex: 1; }
.comment-text { font-size: 14px; line-height: 1.5; }
.comment-date { font-size: 11px; color: var(--muted); margin-top: 4px; display: block; }
.comment-input-row { display: flex; gap: 10px; }

/* Кнопки */
.icon-btn {
  background: none;
  border: none;
  color: var(--muted);
  cursor: pointer;
  font-size: 13px;
  padding: 4px 8px;
  border-radius: 6px;
  transition: all .2s;
  font-family: 'DM Sans', sans-serif;
}
.icon-btn:hover { color: var(--text); background: var(--surface2); }
.icon-btn.danger:hover { color: var(--danger); background: rgba(248,113,113,.1); }
.btn.sm { padding: 6px 12px; font-size: 12px; }

/* Правая колонка */
.tv-sidebar {}
.meta-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  position: sticky;
  top: 90px;
}
.meta-card h4 { font-size: 14px; font-weight: 700; color: var(--muted); text-transform: uppercase; letter-spacing: .5px; }
.meta-row { display: flex; align-items: center; justify-content: space-between; font-size: 13px; }
.meta-label { color: var(--muted); }
.meta-muted { color: var(--muted); }
.meta-edit { border-top: 1px solid var(--border); padding-top: 14px; }

.stage-pill {
  padding: 3px 10px;
  border-radius: 99px;
  font-size: 12px;
  font-weight: 500;
  border: 1px solid;
}
.priority-badge.lg { font-size: 12px; padding: 4px 12px; border-radius: 99px; font-weight: 600; white-space: nowrap; }
.priority-badge.sm { font-size: 11px; padding: 2px 8px; border-radius: 99px; font-weight: 600; }

.overdue { color: var(--danger); }
.empty-sub { text-align: center; color: var(--muted); font-size: 13px; padding: 20px; }

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 24px;
}

.loading-screen {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100vh;
  color: var(--muted);
  font-size: 16px;
}

/* Связанные задачи */
.linked-list { display: flex; flex-direction: column; gap: 8px; }

.linked-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 10px 14px;
  cursor: pointer;
  transition: border-color .2s;
}
.linked-row:hover { border-color: var(--accent); }
.linked-left { display: flex; align-items: center; gap: 8px; }
.linked-right { display: flex; align-items: center; gap: 8px; }
.linked-id { font-size: 11px; color: var(--muted); font-weight: 500; }
.linked-title { font-size: 14px; font-weight: 500; }

/* Модалка связанных задач */
.modal-lg { max-width: 560px; }

.link-tasks-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
  max-height: 360px;
  overflow-y: auto;
  margin-bottom: 16px;
}

.link-task-row {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px;
  border-radius: 9px;
  border: 1px solid var(--border);
  cursor: pointer;
  transition: all .15s;
}
.link-task-row:hover { background: var(--surface2); }
.link-task-row.selected { border-color: var(--accent); background: rgba(91,127,255,.08); }
.link-task-title { flex: 1; font-size: 13px; }

.checkbox {
  width: 17px;
  height: 17px;
  border-radius: 5px;
  flex-shrink: 0;
  border: 1.5px solid var(--border);
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all .2s;
  color: #fff;
}
.checkbox.checked { background: var(--accent); border-color: var(--accent); }
.checkbox svg { width: 10px; height: 10px; }
</style>
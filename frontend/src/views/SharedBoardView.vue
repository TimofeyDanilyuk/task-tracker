<template>
  <div class="board-layout">
    <div class="sidebar-overlay" :class="{ active: sidebarOpen }" @click="sidebarOpen = false"></div>

    <aside class="sidebar" :class="{ open: sidebarOpen }">
      <div class="sidebar-logo">
        <span class="logo-icon">⬡</span>
        <span class="logo-text">Taskflow</span>
        <button class="sidebar-close" @click="sidebarOpen = false">✕</button>
      </div>
      <nav class="sidebar-nav">
        <a class="nav-item" @click="router.push('/'); sidebarOpen = false"><span>⊞</span> Моя доска</a>
        <a class="nav-item" @click="router.push('/todo'); sidebarOpen = false"><span>✓</span> ToDo</a>
        <a class="nav-item" @click="router.push('/calendar'); sidebarOpen = false"><span>📅</span> Календарь</a>
        <a class="nav-item" @click="router.push('/friends'); sidebarOpen = false">
          <span>👥</span> Друзья
          <span v-if="friendsStore.requestsCount > 0" class="nav-badge">{{ friendsStore.requestsCount }}</span>
        </a>
        <div class="nav-section-label">Общие доски</div>
        
         <a v-for="board in boardsStore.boards"
          :key="board.id"
          class="nav-item nav-board-item"
          :class="{ active: board.id === currentBoardId }"
          @click="router.push(`/board/${board.id}`); sidebarOpen = false"
        >
          <span>⊞</span>
          <span class="nav-board-name">{{ board.name }}</span>
          <span class="nav-board-role" v-if="board.myRole === 'Admin'">★</span>
        </a>
        <a class="nav-item nav-add-board" @click="showCreateBoard = true">
          <span>＋</span> Новая доска
        </a>
      </nav>
      <div class="user-badge">
        <span class="user-name">👤 {{ authStore.username }}</span>
        <button class="logout-btn" @click="logout">Выйти</button>
      </div>
    </aside>

    <main class="board-main">
      <div v-if="loading" class="empty-state">Загрузка...</div>
      <template v-else>
        <header class="board-header">
          <div class="board-header-left">
            <button class="hamburger" @click="sidebarOpen = true">
              <span></span><span></span><span></span>
            </button>
            <div>
              <h1 class="board-title">{{ board?.name }}</h1>
              <p class="board-sub">{{ board?.owner?.username }} · {{ board?.membersCount }} участников</p>
            </div>
          </div>
          <div class="header-actions">
            <button v-if="isAdmin" class="btn btn-ghost" @click="showSettings = true">⚙ Настройки</button>
            <button v-if="isAdmin" class="btn btn-ghost" @click="showStagesModal = true">◈ Этапы</button>
            <button class="btn btn-ghost" @click="showOnlyMyTasks = !showOnlyMyTasks" :class="{ active: showOnlyMyTasks }">
              {{ showOnlyMyTasks ? '👤 Все задачи' : '👤 Мои задачи' }}
            </button>
            <button class="btn btn-primary" @click="showTaskModal = true">+ Задача</button>
          </div>
        </header>

        <div class="kanban-board">
          <!-- Без этапа -->
          <div class="kanban-lane">
            <div class="lane-header">
              <div class="lane-title-row">
                <span class="lane-dot" style="background:#6b7280"></span>
                <span class="lane-name">Без этапа</span>
                <span class="lane-count">{{ filteredTasks.filter(t => !t.stageId).length }}</span>
              </div>
            </div>
            <draggable
              class="lane-cards"
              :list="filteredTasks.filter(t => !t.stageId)"
              group="tasks"
              item-key="id"
              @change="onDragEnd($event, null)"
              @start="isDragging = true"
              @end="isDragging = false"
            >
              <template #item="{ element }">
                <TaskCard
                  :task="element"
                  :stages="boardStages"
                  @click="openTask(element.id)"
                  @delete="isAdmin ? deleteTask(element.id) : null"
                />
              </template>
              <template #footer>
                <div v-if="filteredTasks.filter(t => !t.stageId).length === 0 && !isDragging" class="lane-empty">
                  Перетащите задачу сюда
                </div>
              </template>
            </draggable>
          </div>

          <!-- По этапам -->
          <div
            v-for="stage in boardStages"
            :key="stage.id"
            class="kanban-lane"
            :style="`--lane-color: ${stage.color}`"
          >
            <div class="lane-header">
              <div class="lane-title-row">
                <span class="lane-dot" :style="`background:${stage.color}`"></span>
                <span class="lane-name">{{ stage.name }}</span>
                <span class="lane-count">{{ filteredTasks.filter(t => t.stageId === stage.id).length }}</span>
                <span v-if="stage.isFinal" class="lane-final">завершающий</span>
              </div>
              <div class="lane-line" :style="`background:${stage.color}`"></div>
            </div>
            <draggable
              class="lane-cards"
              :list="filteredTasks.filter(t => t.stageId === stage.id)"
              group="tasks"
              item-key="id"
              @change="onDragEnd($event, stage.id)"
              @start="isDragging = true"
              @end="isDragging = false"
            >
              <template #item="{ element }">
                <TaskCard
                  :task="element"
                  :stages="boardStages"
                  @click="openTask(element.id)"
                  @delete="isAdmin ? deleteTask(element.id) : null"
                />
              </template>
              <template #footer>
                <div v-if="filteredTasks.filter(t => t.stageId === stage.id).length === 0 && !isDragging" class="lane-empty">
                  Перетащите задачу сюда
                </div>
              </template>
            </draggable>
          </div>

          <div v-if="boardStages.length === 0 && boardTasks.length === 0" class="board-empty">
            <div class="empty-icon">◎</div>
            <p>Нет задач и этапов</p>
            <div style="display:flex;gap:10px;justify-content:center;margin-top:8px" v-if="isAdmin">
              <button class="btn btn-ghost" @click="showStagesModal = true">+ Этап</button>
              <button class="btn btn-primary" @click="showTaskModal = true">+ Задача</button>
            </div>
          </div>
        </div>
      </template>
    </main>

    <!-- Модалка задачи -->
    <TaskModal
      v-if="showTaskModal"
      :stages="boardStages"
      :board-id="currentBoardId"
      :members="boardMembers"
      @close="showTaskModal = false"
      @created="onTaskCreated"
    />

    <!-- Модалка этапов доски -->
    <div class="modal-overlay" v-if="showStagesModal" @click.self="showStagesModal = false">
      <div class="modal">
        <div class="modal-header">
          <h2>Этапы доски</h2>
          <button class="btn btn-ghost" @click="showStagesModal = false">✕</button>
        </div>
        <div class="stages-list">
          <div v-for="s in boardStages" :key="s.id" class="stage-row">
            <span class="stage-dot" :style="`background:${s.color}`"></span>
            <span class="stage-name">{{ s.name }}</span>
            <button class="delete-btn" @click="removeStage(s.id)">✕</button>
          </div>
        </div>
        <div class="form-row" style="margin-top:16px">
          <input v-model="newStage.name" placeholder="Название этапа" />
          <input type="color" v-model="newStage.color" style="width:44px;padding:2px" />
          <button class="btn btn-primary" @click="addStage" :disabled="!newStage.name.trim()">+</button>
        </div>
      </div>
    </div>

    <!-- Настройки доски -->
    <div class="modal-overlay" v-if="showSettings" @click.self="showSettings = false">
      <div class="modal">
        <div class="modal-header">
          <h2>Настройки доски</h2>
          <button class="btn btn-ghost" @click="showSettings = false">✕</button>
        </div>

        <div class="form-group">
          <label>Участники</label>
          <div class="members-list">
            <!-- Владелец -->
            <div class="member-row">
              <div class="member-avatar">{{ board?.owner?.username[0].toUpperCase() }}</div>
              <span class="member-name">{{ board?.owner?.username }}</span>
              <span class="member-role owner">Владелец</span>
            </div>
            <!-- Остальные -->
            <div v-for="m in boardMembers" :key="m.id" class="member-row">
              <div class="member-avatar">{{ m.username[0].toUpperCase() }}</div>
              <span class="member-name">{{ m.username }}</span>
              <select
                class="role-select"
                :value="m.role"
                @change="setRole(m.userId, $event.target.value)"
              >
                <option value="0">Участник</option>
                <option value="1">Админ</option>
              </select>
              <button class="delete-btn" @click="removeMember(m.userId)">✕</button>
            </div>
          </div>
        </div>

        <div class="form-group">
          <label>Добавить участника из друзей</label>
          <div class="add-member-row">
            <select v-model="selectedFriend">
              <option :value="null">— Выбери друга —</option>
              <option
                v-for="f in availableFriends"
                :key="f.user.id"
                :value="f.user.id"
              >{{ f.user.username }}</option>
            </select>
            <button class="btn btn-primary" @click="addMember" :disabled="!selectedFriend">+</button>
          </div>
        </div>

        <div class="modal-actions">
          <button class="btn btn-danger" @click="deleteBoard">Удалить доску</button>
        </div>
      </div>
    </div>

    <!-- Модалка создания доски -->
    <div class="modal-overlay" v-if="showCreateBoard" @click.self="showCreateBoard = false">
      <div class="modal">
        <div class="modal-header">
          <h2>Новая доска</h2>
          <button class="btn btn-ghost" @click="showCreateBoard = false">✕</button>
        </div>
        <div class="form-group">
          <label>Название</label>
          <input v-model="newBoardName" placeholder="Название доски..." @keyup.enter="createBoard" />
        </div>
        <div class="modal-actions">
          <button class="btn btn-ghost" @click="showCreateBoard = false">Отмена</button>
          <button class="btn btn-primary" @click="createBoard" :disabled="!newBoardName.trim()">Создать</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useBoardsStore } from '@/stores/boards'
import { useFriendsStore } from '@/stores/friends'
import { useAuthStore } from '@/stores/auth'
import draggable from 'vuedraggable'
import TaskCard from '@/components/TaskCard.vue'
import TaskModal from '@/components/TaskModal.vue'
import api from '@/api'

const router = useRouter()
const route = useRoute()
const boardsStore = useBoardsStore()
const friendsStore = useFriendsStore()
const authStore = useAuthStore()

const loading = ref(true)
const sidebarOpen = ref(false)
const isDragging = ref(false)
const showTaskModal = ref(false)
const showStagesModal = ref(false)
const showSettings = ref(false)
const showCreateBoard = ref(false)
const newBoardName = ref('')
const selectedFriend = ref(null)
const newStage = ref({ name: '', color: '#5b7fff' })
const showOnlyMyTasks = ref(false)

const currentBoardId = computed(() => parseInt(route.params.id))
const board = computed(() => boardsStore.current)
const boardStages = computed(() => boardsStore.currentStages)
const boardTasks = computed(() => boardsStore.currentTasks)
const boardMembers = computed(() => board.value?.members ?? [])
const isAdmin = computed(() => board.value?.myRole === 'Admin')

const filteredTasks = computed(() => {
  if (!showOnlyMyTasks.value) return boardTasks.value
  const myId = authStore.userId
  console.log('Filtering my tasks, myId:', myId, 'tasks:', boardTasks.value.map(t => ({ id: t.id, assignedUserId: t.assignedUserId })))
  return boardTasks.value.filter(t => t.assignedUserId === myId)
})

const availableFriends = computed(() => {
  const memberIds = new Set(boardMembers.value.map(m => m.userId))
  memberIds.add(board.value?.owner?.id)
  return friendsStore.friends.filter(f => !memberIds.has(f.user.id))
})

function logout() { authStore.logout(); router.push('/auth') }

async function load() {
  loading.value = true
  await Promise.all([
    boardsStore.fetchOne(currentBoardId.value),
    boardsStore.fetchStages(currentBoardId.value),
    boardsStore.fetchTasks(currentBoardId.value),
    boardsStore.fetchAll(),
    friendsStore.fetchFriends(),
    friendsStore.fetchRequestsCount()
  ])
  loading.value = false
}

onMounted(load)
watch(() => route.params.id, (id) => { if (id) load() })

function openTask(id) { router.push(`/task/${id}`) }

async function onDragEnd(evt, targetStageId) {
  const element = evt.added?.element
  if (!element) return
  const task = boardTasks.value.find(t => t.id === element.id)
  if (task) task.stageId = targetStageId

  await boardsStore.changeStage(element.id, targetStageId)

  const targetStage = boardStages.value.find(s => s.id === targetStageId)
  if (targetStage?.isFinal && !element.isDone) {
    await api.patch(`/tasks/${element.id}/done`)
  }
  if (!targetStage && element.isDone || (targetStage && !targetStage.isFinal && element.isDone)) {
    await api.patch(`/tasks/${element.id}/done`)
  }

  await boardsStore.fetchTasks(currentBoardId.value)
}

async function deleteTask(id) {
  await api.delete(`/tasks/${id}`)
  await boardsStore.fetchTasks(currentBoardId.value)
}

async function onTaskCreated() {
  showTaskModal.value = false
  await boardsStore.fetchTasks(currentBoardId.value)
}

async function addStage() {
  await boardsStore.createStage(currentBoardId.value, { ...newStage.value })
  newStage.value = { name: '', color: '#5b7fff' }
}

async function removeStage(id) {
  await boardsStore.removeStage(id)
}

async function addMember() {
  if (!selectedFriend.value) return
  await boardsStore.addMember(currentBoardId.value, selectedFriend.value)
  selectedFriend.value = null
  await boardsStore.fetchOne(currentBoardId.value)
}

async function removeMember(userId) {
  await boardsStore.removeMember(currentBoardId.value, userId)
  await boardsStore.fetchOne(currentBoardId.value)
}

async function setRole(userId, role) {
  await boardsStore.setRole(currentBoardId.value, userId, parseInt(role))
  await boardsStore.fetchOne(currentBoardId.value)
}

async function deleteBoard() {
  if (!confirm('Удалить доску?')) return
  await boardsStore.remove(currentBoardId.value)
  router.push('/')
}

async function createBoard() {
  if (!newBoardName.value.trim()) return
  const board = await boardsStore.create(newBoardName.value.trim())
  newBoardName.value = ''
  showCreateBoard.value = false
  router.push(`/board/${board.id}`)
}
</script>

<style scoped>
.lane-final { font-size: 10px; color: var(--muted); margin-left: 4px; }
.board-layout { display: flex; min-height: 100vh; }
.sidebar { width: 230px; background: var(--surface); border-right: 1px solid var(--border); display: flex; flex-direction: column; padding: 28px 16px; position: sticky; top: 0; height: 100vh; flex-shrink: 0; z-index: 100; overflow-y: auto; }
.sidebar-logo { display: flex; align-items: center; gap: 10px; padding: 0 8px; margin-bottom: 36px; }
.logo-icon { font-size: 24px; color: var(--accent); }
.logo-text { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 20px; letter-spacing: -0.5px; flex: 1; }
.sidebar-close { display: none; background: none; border: none; color: var(--muted); font-size: 18px; cursor: pointer; padding: 4px; }
.sidebar-nav { display: flex; flex-direction: column; gap: 4px; }
.nav-item { display: flex; align-items: center; gap: 10px; padding: 10px 12px; border-radius: 9px; cursor: pointer; font-size: 14px; color: var(--muted); transition: all .2s; }
.nav-item:hover, .nav-item.active { background: var(--surface2); color: var(--text); }
.nav-item.active { color: var(--accent); }
.nav-section-label { font-size: 10px; text-transform: uppercase; letter-spacing: .8px; color: var(--muted); padding: 12px 12px 4px; font-weight: 600; }
.nav-board-name { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.nav-board-role { color: var(--accent); font-size: 12px; }
.nav-add-board { color: var(--accent); opacity: .7; }
.nav-add-board:hover { opacity: 1; }
.nav-badge { margin-left: auto; background: var(--danger); color: #fff; font-size: 11px; font-weight: 700; padding: 1px 7px; border-radius: 99px; }
.user-badge { display: flex; align-items: center; justify-content: space-between; padding: 10px 12px; margin-top: auto; background: var(--surface2); border-radius: 9px; border: 1px solid var(--border); }
.user-name { font-size: 13px; color: var(--muted); }
.logout-btn { background: none; border: none; color: var(--muted); cursor: pointer; font-size: 12px; }
.logout-btn:hover { color: var(--danger); }
.hamburger { display: none; flex-direction: column; gap: 5px; background: none; border: none; cursor: pointer; padding: 6px; border-radius: 8px; }
.hamburger span { display: block; width: 22px; height: 2px; background: var(--text); border-radius: 99px; }
.sidebar-overlay { display: none; position: fixed; inset: 0; background: rgba(0,0,0,.5); z-index: 99; opacity: 0; transition: opacity .25s; }
.sidebar-overlay.active { opacity: 1; }

.board-main { flex: 1; padding: 40px 36px; overflow-y: auto; min-width: 0; }
.board-header { display: flex; align-items: flex-start; justify-content: space-between; margin-bottom: 36px; flex-wrap: wrap; gap: 12px; }
.board-header-left { display: flex; align-items: center; gap: 14px; }
.board-title { font-size: 28px; font-weight: 800; letter-spacing: -0.5px; }
.board-sub { font-size: 13px; color: var(--muted); margin-top: 4px; }
.header-actions { display: flex; gap: 8px; flex-wrap: wrap; }

.kanban-board { display: flex; flex-direction: column; gap: 8px; }
.kanban-lane { background: var(--surface); border: 1px solid var(--border); border-radius: 16px; overflow: hidden; }
.kanban-lane:has(.lane-line) { border-top: 3px solid var(--lane-color, var(--border)); }
.lane-header { padding: 16px 20px 12px; }
.lane-title-row { display: flex; align-items: center; gap: 10px; }
.lane-dot { width: 10px; height: 10px; border-radius: 50%; flex-shrink: 0; }
.lane-name { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 15px; }
.lane-count { background: var(--surface2); border: 1px solid var(--border); border-radius: 99px; font-size: 11px; padding: 1px 8px; color: var(--muted); }
.lane-line { height: 2px; margin-top: 12px; border-radius: 99px; opacity: .5; }
.lane-cards { display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: 12px; padding: 4px 20px 20px; min-height: 80px; }
.lane-empty { grid-column: 1 / -1; text-align: center; padding: 24px; color: var(--muted); font-size: 13px; border: 1px dashed var(--border); border-radius: 10px; }
.board-empty { text-align: center; padding: 80px 20px; color: var(--muted); display: flex; flex-direction: column; align-items: center; gap: 12px; }
.empty-icon { font-size: 48px; opacity: .3; }
.empty-state { text-align: center; padding: 80px; color: var(--muted); }

.stages-list { display: flex; flex-direction: column; gap: 8px; }
.stage-row { display: flex; align-items: center; gap: 10px; padding: 8px; background: var(--surface2); border-radius: 8px; }
.stage-dot { width: 12px; height: 12px; border-radius: 50%; flex-shrink: 0; }
.stage-name { flex: 1; font-size: 14px; }

.members-list { display: flex; flex-direction: column; gap: 8px; margin-bottom: 16px; }
.member-row { display: flex; align-items: center; gap: 10px; padding: 8px; background: var(--surface2); border-radius: 8px; }
.member-avatar { width: 32px; height: 32px; border-radius: 50%; background: var(--accent); color: #fff; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 13px; flex-shrink: 0; }
.member-name { flex: 1; font-size: 14px; }
.member-role { font-size: 11px; padding: 2px 8px; border-radius: 99px; }
.member-role.owner { background: rgba(91,127,255,.15); color: var(--accent); }
.role-select { font-size: 12px; background: var(--surface); border: 1px solid var(--border); border-radius: 6px; padding: 3px 6px; color: var(--text); }
.add-member-row { display: flex; gap: 8px; }
.add-member-row select { flex: 1; }
.delete-btn { background: none; border: none; color: var(--muted); cursor: pointer; font-size: 12px; padding: 2px 6px; border-radius: 6px; transition: all .2s; }
.delete-btn:hover { color: var(--danger); background: rgba(248,113,113,.1); }
.btn-danger { background: rgba(248,113,113,.15); color: var(--danger); border: 1px solid var(--danger); }
.btn-danger:hover { background: var(--danger); color: #fff; }

@media (max-width: 768px) {
  .hamburger { display: flex; }
  .sidebar-close { display: block; }
  .sidebar-overlay { display: block; pointer-events: none; }
  .sidebar-overlay.active { pointer-events: all; }
  .sidebar { position: fixed; left: -260px; top: 0; height: 100vh; width: 240px; transition: left .25s ease; }
  .sidebar.open { left: 0; box-shadow: 4px 0 24px rgba(0,0,0,.4); }
  .board-main { padding: 20px 16px; }
  .board-title { font-size: 20px; }
  .lane-cards { grid-template-columns: 1fr; padding: 4px 12px 12px; }
  .header-actions { width: 100%; justify-content: flex-end; }
}
</style>
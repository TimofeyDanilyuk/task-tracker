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
        <a class="nav-item" @click="router.push('/'); sidebarOpen = false"><span>⊞</span> Доска</a>
        <a class="nav-item" @click="router.push('/todo'); sidebarOpen = false"><span>✓</span> ToDo</a>
        <a class="nav-item active"><span>👥</span> Друзья</a>
      </nav>
      <div class="user-badge">
        <span class="user-name">👤 {{ authStore.username }}</span>
        <button class="logout-btn" @click="logout">Выйти</button>
      </div>
    </aside>

    <main class="board-main">
      <header class="board-header">
        <div class="board-header-left">
          <button class="hamburger" @click="sidebarOpen = true">
            <span></span><span></span><span></span>
          </button>
          <h1 class="board-title">Друзья</h1>
        </div>
      </header>

      <!-- Вкладки -->
      <div class="tabs">
        <button class="tab" :class="{ active: tab === 'friends' }" @click="tab = 'friends'">
          Друзья <span class="tab-count">{{ friendsStore.friends.length }}</span>
        </button>
        <button class="tab" :class="{ active: tab === 'requests' }" @click="tab = 'requests'; loadRequests()">
          Заявки
          <span class="tab-count" :class="{ danger: friendsStore.requests.length > 0 }">
            {{ friendsStore.requests.length }}
          </span>
        </button>
      </div>

      <!-- Друзья -->
      <div v-if="tab === 'friends'">
        <div class="add-friend-row">
          <input v-model="searchUsername" placeholder="Введите username..." @keyup.enter="sendRequest" />
          <button class="btn btn-primary" @click="sendRequest" :disabled="!searchUsername.trim()">
            Добавить
          </button>
        </div>
        <p v-if="addError" class="error-msg">{{ addError }}</p>
        <p v-if="addSuccess" class="success-msg">{{ addSuccess }}</p>

        <div v-if="friendsStore.friends.length === 0" class="empty-state" style="padding: 60px 0">
          <div class="empty-icon">👥</div>
          <p>Друзей пока нет</p>
        </div>

        <div class="friends-list" v-else>
          <div v-for="f in friendsStore.friends" :key="f.id" class="friend-card">
            <div class="friend-avatar">{{ f.user.username[0].toUpperCase() }}</div>
            <span class="friend-name">{{ f.user.username }}</span>
            <button class="delete-btn" @click="friendsStore.remove(f.id)">✕</button>
          </div>
        </div>
      </div>

      <!-- Заявки -->
      <div v-if="tab === 'requests'">
        <div v-if="friendsStore.requests.length === 0" class="empty-state" style="padding: 60px 0">
          <div class="empty-icon">📭</div>
          <p>Заявок нет</p>
        </div>

        <div class="friends-list" v-else>
          <div v-for="r in friendsStore.requests" :key="r.id" class="friend-card">
            <div class="friend-avatar">{{ r.username[0].toUpperCase() }}</div>
            <span class="friend-name">{{ r.username }}</span>
            <div class="request-actions">
              <button class="btn btn-primary btn-sm" @click="friendsStore.accept(r.id)">✓</button>
              <button class="btn btn-ghost btn-sm" @click="friendsStore.decline(r.id)">✕</button>
            </div>
          </div>
        </div>
      </div>

    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useFriendsStore } from '@/stores/friends'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const friendsStore = useFriendsStore()
const authStore = useAuthStore()
const sidebarOpen = ref(false)
const tab = ref('friends')
const searchUsername = ref('')
const addError = ref('')
const addSuccess = ref('')

function logout() {
  authStore.logout()
  router.push('/auth')
}

async function loadRequests() {
  await friendsStore.fetchRequests()
}

async function sendRequest() {
  addError.value = ''
  addSuccess.value = ''
  try {
    await friendsStore.sendRequest(searchUsername.value.trim())
    addSuccess.value = `Заявка отправлена пользователю ${searchUsername.value}`
    searchUsername.value = ''
  } catch (e) {
    addError.value = e.response?.data || 'Ошибка при отправке заявки'
  }
}

onMounted(async () => {
  await Promise.all([friendsStore.fetchFriends(), friendsStore.fetchRequests()])
})
</script>

<style scoped>
.board-layout { display: flex; min-height: 100vh; }
.sidebar { width: 230px; background: var(--surface); border-right: 1px solid var(--border); display: flex; flex-direction: column; padding: 28px 16px; position: sticky; top: 0; height: 100vh; flex-shrink: 0; z-index: 100; }
.sidebar-logo { display: flex; align-items: center; gap: 10px; padding: 0 8px; margin-bottom: 36px; }
.logo-icon { font-size: 24px; color: var(--accent); }
.logo-text { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 20px; letter-spacing: -0.5px; flex: 1; }
.sidebar-close { display: none; background: none; border: none; color: var(--muted); font-size: 18px; cursor: pointer; padding: 4px; }
.sidebar-nav { display: flex; flex-direction: column; gap: 4px; }
.nav-item { display: flex; align-items: center; gap: 10px; padding: 10px 12px; border-radius: 9px; cursor: pointer; font-size: 14px; color: var(--muted); transition: all .2s; }
.nav-item:hover, .nav-item.active { background: var(--surface2); color: var(--text); }
.nav-item.active { color: var(--accent); }
.user-badge { display: flex; align-items: center; justify-content: space-between; padding: 10px 12px; margin-top: auto; background: var(--surface2); border-radius: 9px; border: 1px solid var(--border); }
.user-name { font-size: 13px; color: var(--muted); }
.logout-btn { background: none; border: none; color: var(--muted); cursor: pointer; font-size: 12px; transition: color .2s; }
.logout-btn:hover { color: var(--danger); }
.hamburger { display: none; flex-direction: column; gap: 5px; background: none; border: none; cursor: pointer; padding: 6px; border-radius: 8px; }
.hamburger span { display: block; width: 22px; height: 2px; background: var(--text); border-radius: 99px; }
.sidebar-overlay { display: none; position: fixed; inset: 0; background: rgba(0,0,0,.5); z-index: 99; opacity: 0; transition: opacity .25s; }
.sidebar-overlay.active { opacity: 1; }

.board-main { flex: 1; padding: 40px 36px; overflow-y: auto; min-width: 0; }
.board-header { display: flex; align-items: center; margin-bottom: 32px; }
.board-header-left { display: flex; align-items: center; gap: 14px; }
.board-title { font-size: 28px; font-weight: 800; letter-spacing: -0.5px; }

.tabs { display: flex; gap: 4px; margin-bottom: 24px; border-bottom: 1px solid var(--border); }
.tab { background: none; border: none; padding: 10px 20px; font-size: 14px; color: var(--muted); cursor: pointer; border-bottom: 2px solid transparent; margin-bottom: -1px; transition: all .2s; display: flex; align-items: center; gap: 8px; }
.tab.active { color: var(--accent); border-bottom-color: var(--accent); }
.tab-count { background: var(--surface2); border: 1px solid var(--border); border-radius: 99px; font-size: 11px; padding: 1px 7px; }
.tab-count.danger { background: rgba(248,113,113,.15); border-color: var(--danger); color: var(--danger); }

.add-friend-row { display: flex; gap: 10px; margin-bottom: 12px; }
.add-friend-row input { flex: 1; }

.friends-list { display: flex; flex-direction: column; gap: 8px; max-width: 500px; }
.friend-card { display: flex; align-items: center; gap: 12px; padding: 12px 16px; background: var(--surface); border: 1px solid var(--border); border-radius: 12px; }
.friend-avatar { width: 36px; height: 36px; border-radius: 50%; background: var(--accent); color: #fff; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 15px; flex-shrink: 0; }
.friend-name { flex: 1; font-size: 14px; font-weight: 500; }
.request-actions { display: flex; gap: 6px; }
.btn-sm { padding: 4px 10px; font-size: 12px; }

.error-msg { color: var(--danger); font-size: 13px; margin-bottom: 12px; }
.success-msg { color: var(--success); font-size: 13px; margin-bottom: 12px; }
.empty-state { display: flex; flex-direction: column; align-items: center; gap: 14px; color: var(--muted); text-align: center; }
.empty-icon { font-size: 44px; opacity: .25; }
.delete-btn { background: none; border: none; color: var(--muted); cursor: pointer; font-size: 12px; padding: 2px 6px; border-radius: 6px; transition: all .2s; }
.delete-btn:hover { color: var(--danger); background: rgba(248,113,113,.1); }

@media (max-width: 768px) {
  .hamburger { display: flex; }
  .sidebar-close { display: block; }
  .sidebar-overlay { display: block; pointer-events: none; }
  .sidebar-overlay.active { pointer-events: all; }
  .sidebar { position: fixed; left: -260px; top: 0; height: 100vh; width: 240px; transition: left .25s ease; }
  .sidebar.open { left: 0; box-shadow: 4px 0 24px rgba(0,0,0,.4); }
  .board-main { padding: 20px 16px; }
  .board-title { font-size: 20px; }
  .add-friend-row { flex-direction: column; }
}
</style>
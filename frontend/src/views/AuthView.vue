<template>
  <div class="auth-page">
    <div class="auth-card">
      <div class="auth-logo">
        <span class="logo-icon">⬡</span>
        <span class="logo-text">Taskflow</span>
      </div>

      <div class="auth-tabs">
        <button :class="['tab', { active: mode === 'login' }]" @click="mode = 'login'">Вход</button>
        <button :class="['tab', { active: mode === 'register' }]" @click="mode = 'register'">Регистрация</button>
      </div>

      <transition name="fade" mode="out-in">
        <div :key="mode">
          <h2 class="auth-title">{{ mode === 'login' ? 'Добро пожаловать' : 'Создать аккаунт' }}</h2>
          <p class="auth-sub">{{ mode === 'login' ? 'Войди в свой аккаунт' : 'Зарегистрируйся чтобы начать' }}</p>

          <div class="form-group">
            <label>Логин</label>
            <input v-model="form.username" placeholder="Введи логин" @keyup.enter="submit" />
          </div>
          <div class="form-group">
            <label>Пароль</label>
            <input v-model="form.password" type="password" placeholder="Введи пароль" @keyup.enter="submit" />
          </div>

          <p v-if="error" class="auth-error">{{ error }}</p>

          <button class="btn btn-primary auth-btn" :disabled="loading" @click="submit">
            {{ loading ? 'Загрузка...' : (mode === 'login' ? 'Войти' : 'Зарегистрироваться') }}
          </button>
        </div>
      </transition>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const mode = ref('login')
const loading = ref(false)
const error = ref('')
const form = reactive({ username: '', password: '' })

async function submit() {
  error.value = ''
  if (!form.username.trim() || !form.password.trim()) {
    error.value = 'Заполни все поля'
    return
  }
  loading.value = true
  try {
    if (mode.value === 'login') await authStore.login(form.username, form.password)
    else await authStore.register(form.username, form.password)
    router.push('/')
  } catch (e) {
    error.value = e.response?.data || 'Что-то пошло не так'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--bg);
  background-image: radial-gradient(ellipse at 20% 50%, rgba(91,127,255,.08) 0%, transparent 60%),
                    radial-gradient(ellipse at 80% 20%, rgba(167,139,250,.06) 0%, transparent 50%);
}

.auth-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 24px;
  padding: 44px 40px;
  width: 420px;
  box-shadow: 0 24px 80px rgba(0,0,0,.5);
}

.auth-logo {
  display: flex; align-items: center; gap: 10px;
  justify-content: center; margin-bottom: 32px;
}
.logo-icon { font-size: 28px; color: var(--accent); }
.logo-text { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 24px; }

.auth-tabs {
  display: flex;
  background: var(--surface2);
  border-radius: 10px;
  padding: 4px;
  margin-bottom: 28px;
}
.tab {
  flex: 1; padding: 9px; border: none; background: transparent;
  color: var(--muted); font-family: 'DM Sans', sans-serif;
  font-size: 14px; font-weight: 500; border-radius: 7px;
  cursor: pointer; transition: all .2s;
}
.tab.active { background: var(--surface); color: var(--text); box-shadow: 0 2px 8px rgba(0,0,0,.3); }

.auth-title { font-size: 20px; font-weight: 800; margin-bottom: 4px; }
.auth-sub { font-size: 13px; color: var(--muted); margin-bottom: 24px; }

.auth-error {
  color: var(--danger); font-size: 13px;
  margin-bottom: 12px; padding: 10px 14px;
  background: rgba(248,113,113,.1); border-radius: 8px;
}

.auth-btn { width: 100%; justify-content: center; padding: 12px; font-size: 15px; margin-top: 8px; }

.fade-enter-active, .fade-leave-active { transition: opacity .15s, transform .15s; }
.fade-enter-from { opacity: 0; transform: translateY(6px); }
.fade-leave-to { opacity: 0; transform: translateY(-6px); }
</style>
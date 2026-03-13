import { defineStore } from 'pinia'
import api from '@/api'

function parseJwt(token) {
  try {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const jsonPayload = decodeURIComponent(atob(base64).split('').map(c =>
      '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
    ).join(''))
    return JSON.parse(jsonPayload)
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', {
  state: () => {
    const token = localStorage.getItem('token')
    const username = localStorage.getItem('username')
    let userId = localStorage.getItem('userId') ? parseInt(localStorage.getItem('userId')) : null
    if (token && !userId) {
      const payload = parseJwt(token)
      userId = payload?.userId ? parseInt(payload.userId) : null
      if (userId) localStorage.setItem('userId', userId)
    }
    return {
      token,
      username,
      userId
    }
  },
  getters: {
    isLoggedIn: (s) => !!s.token
  },
  actions: {
    async login(username, password) {
      const { data } = await api.post('/auth/login', { username, password })
      this.token = data.token
      this.username = username
      const payload = parseJwt(data.token)
      this.userId = payload?.userId ? parseInt(payload.userId) : null
      localStorage.setItem('token', data.token)
      localStorage.setItem('username', username)
      if (this.userId) localStorage.setItem('userId', this.userId)
    },
    async register(username, password) {
      const { data } = await api.post('/auth/register', { username, password })
      this.token = data.token
      this.username = username
      const payload = parseJwt(data.token)
      this.userId = payload?.userId ? parseInt(payload.userId) : null
      localStorage.setItem('token', data.token)
      localStorage.setItem('username', username)
      if (this.userId) localStorage.setItem('userId', this.userId)
    },
    logout() {
      this.token = null
      this.username = null
      this.userId = null
      localStorage.removeItem('token')
      localStorage.removeItem('username')
      localStorage.removeItem('userId')
    }
  }
})
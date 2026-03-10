import { defineStore } from 'pinia'
import api from '@/api'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') || null,
    username: localStorage.getItem('username') || null
  }),
  getters: {
    isLoggedIn: (s) => !!s.token
  },
  actions: {
    async login(username, password) {
      const { data } = await api.post('/auth/login', { username, password })
      this.token = data.token
      this.username = username
      localStorage.setItem('token', data.token)
      localStorage.setItem('username', username)
    },
    async register(username, password) {
      const { data } = await api.post('/auth/register', { username, password })
      this.token = data.token
      this.username = username
      localStorage.setItem('token', data.token)
      localStorage.setItem('username', username)
    },
    logout() {
      this.token = null
      this.username = null
      localStorage.removeItem('token')
      localStorage.removeItem('username')
    }
  }
})
import { defineStore } from 'pinia'
import api from '@/api'

export const useBoardsStore = defineStore('boards', {
  state: () => ({
    boards: [],
    current: null,
    currentStages: [],
    currentTasks: []
  }),
  actions: {
    async fetchAll() {
      const { data } = await api.get('/boards')
      this.boards = data
    },
    async fetchOne(id) {
      const { data } = await api.get(`/boards/${id}`)
      this.current = data
    },
    async fetchStages(boardId) {
      const { data } = await api.get(`/stages/board/${boardId}`)
      this.currentStages = data
    },
    async fetchTasks(boardId) {
      const { data } = await api.get(`/tasks/board/${boardId}`)
      this.currentTasks = data
    },
    async create(name) {
      const { data } = await api.post('/boards', { name })
      this.boards.push(data)
      return data
    },
    async remove(id) {
      await api.delete(`/boards/${id}`)
      this.boards = this.boards.filter(b => b.id !== id)
    },
    async addMember(boardId, userId) {
      await api.post(`/boards/${boardId}/members`, { userId })
    },
    async removeMember(boardId, userId) {
      await api.delete(`/boards/${boardId}/members/${userId}`)
    },
    async setRole(boardId, userId, role) {
      await api.patch(`/boards/${boardId}/members/${userId}/role`, { role })
    },
    async assignTask(boardId, taskId, userId) {
      await api.patch(`/boards/${boardId}/tasks/${taskId}/assign`, { userId })
    },
    async changeStage(taskId, stageId) {
      await api.patch(`/tasks/${taskId}/stage`, { stageId })
      const task = this.currentTasks.find(t => t.id === taskId)
      if (task) task.stageId = stageId
    },
    async createStage(boardId, stage) {
      const { data } = await api.post(`/stages/board/${boardId}`, stage)
      this.currentStages.push(data)
    },
    async removeStage(id) {
      await api.delete(`/stages/${id}`)
      this.currentStages = this.currentStages.filter(s => s.id !== id)
    }
  }
})
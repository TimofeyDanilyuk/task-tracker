import { defineStore } from 'pinia'
import api from '@/api'

export const useStagesStore = defineStore('stages', {
  state: () => ({ stages: [] }),
  actions: {
    async fetch() {
      const { data } = await api.get('/stages')
      this.stages = data
    },
    async create(stage) {
      const { data } = await api.post('/stages', stage)
      this.stages.push(data)
    },
    async update(id, stage) {
      const { data } = await api.put(`/stages/${id}`, stage)
      const i = this.stages.findIndex(s => s.id === id)
      if (i !== -1) this.stages[i] = data
    },
    async remove(id) {
      await api.delete(`/stages/${id}`)
      this.stages = this.stages.filter(s => s.id !== id)
    }
  }
})
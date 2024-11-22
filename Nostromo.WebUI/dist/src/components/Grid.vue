<template>
    <div class="grid-container">
      <div v-for="(item, index) in items" :key="index" class="grid-item">
        <p>{{ item.name }}</p>
      </div>
    </div>
  </template>
  

  <script>
export default {
  data() {
    return {
      items: [
        { name: 'Item 1' },
        { name: 'Item 2' },
        { name: 'Item 3' },
        { name: 'Item 4' },
        { name: 'Item 5' },
        { name: 'Item 6' }
      ]
    };
  }
};
</script>




<style scoped>
.grid-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 20px;
  padding: 20px;
}

.grid-item {
  background-color: #f4f4f4;
  padding: 20px;
  text-align: center;
  border-radius: 8px;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
}
</style>

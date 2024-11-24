<template>
    <div class="parent">
      <div class="box">
        <h2>This is a Parent Box Section</h2>
        <p>Content inside the box goes here.</p>
        <MovieContainer>Movie</MovieContainer>
      </div>
      
      
      <div class="child-box">
        <div class="movie-box">
          <h2>This is a Child Box Section</h2>
          <p>Content inside the child box goes here.</p>
          <MovieContainer></MovieContainer>
        </div>
        <div class="movie-box">
          <h2>Another Child Box Section</h2>
          <p>Additional content inside this child box goes here.</p>
          <MovieContainer></MovieContainer>
        </div>
      </div>
    </div>
  </template>
  
  <script>
  import MovieContainer from './MovieContainer.vue';
  
  export default {
    name: 'BoxSection',
    components: {
      MovieContainer,
    },
  };
  </script>
  
  <style scoped>
  .parent {
    display: flex;
    flex-direction: column;
    height: 100vh;
    align-items: center; /* Center align items */
    padding: 20px;
  }
  
  .box {
    border: 2px solid #ccc;
    padding: 20px;
    margin: 20px 0;
    border-radius: 8px;
    background-color: #f9f9f9;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    flex-grow: 1;
    width: 750px;
  }
  
  .child-box {
    display: flex;
    flex-direction: row; /* Arrange children in a row */
    flex-wrap: wrap; /* Allow wrapping of items to new rows */
    justify-content: center; /* Center child boxes horizontally */
    gap: 20px; /* Add space between child boxes */
  }
  
  .movie-box {
    width: 400px; /* Adjust width for smaller child boxes */
    height: auto; /* Adjust height based on content */
    border: 2px solid #ccc;
    padding: 20px;
    margin: 10px 0;
    border-radius: 8px;
    background-color: #f9f9f9;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  }
  </style>
  
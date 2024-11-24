<template> 
     <div class="child-box">
      <div class="movie-box">
        <h2>This is a Box Section</h2>
        <p>Content inside the box goes here.</p>
        <MovieContainer></MovieContainer>
      </div>
    </div>

</template>




<script>



</script>


<style scoped>
 .child-box{
 
    
    flex-direction: column; /* Stack children vertically */
    

 }
 .movie-box{
    width: 1024px;
    height: 768;
    border: 2px solid #ccc;
    padding: 20px;
    margin: 20px 0;
    border-radius: 8px;
    background-color: #f9f9f9;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
 }


</style>
# Guía para Ejecutar la Aplicación ASP.NET Core

## Pasos para la Ejecución:

1. **Clonar el Repositorio**
   ```bash
   git clone https://github.com/ViCode321/project_DBA_VISO
2. **Ubicar la rama que les corresponde**
   ```bash
   git checkout <NOMBRE_DE_LA_RAMA>   
3. **Sincronizan la rama remota con su rama**
   ```bash
   git pull origin <NOMBRE_DE_LA_RAMA>   
4. **Crean la base de datos, SOLO LAS TABLAS**
5. **Insertar los roles de usuario**
6. **Ubicar el controlador UserController**
   Se encuentra en la capa WebAPP carpeta controller.
7. **Buscar la función Signin y descomentarear la línea de código**
   ```bash
   //CreateAdminUser().
8. **Ejecutar la app**
   Se puede usar fn + F5
   NO HACER LOGIN
10. **Una vez ejecutada la app cerrarla**
11. **Comentarear nuevamente CreateAdminUser()**
12. **Insertar el resto de los registros**
   NO CREAR LOS PROCEDIMIENTOS ALMACENDADOS.
13. **Crear los procedimientos almacenados uno por uno**
14. **Luego la app estaría lista para trabajar**
   NOTA: Si modifica algo en la BD se debe realizar todo el proceso nuevamnte.
15. **Para guardar cambios en el cuadro de texto superior de git**
   ```bash
   git commit -m "mensaje"
   ```
16. **Luego presionar en push**
    ```bash
    git push origin <NOMBRE_DE_LA_RAMA>
   

 

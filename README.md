
# 🐇 PracticaRabbitMQ

Este proyecto demuestra una integración básica de **RabbitMQ** con una aplicación .NET (C#), utilizando contenedores Docker para facilitar la ejecución local.

## 🚀 Tecnologías utilizadas

- .NET 9.0 (OrderConsumer)
- RabbitMQ (con consola de administración)
- Docker y Docker Compose


## 🐳 Cómo levantar el entorno

1. Asegúrate de tener [Docker instalado](https://www.docker.com/products/docker-desktop/).

2. Ejecuta el siguiente comando desde la raíz del proyecto:

```bash
docker-compose up -d --build
```

3. Accede a la consola de RabbitMQ en tu navegador:

```
http://localhost:25672
Usuario: admin
Contraseña: admin
```

## ✅ Estado

Este proyecto es una práctica base para aprendizaje. Puedes ampliarlo agregando más microservicios, colas, o lógica de negocio.

---

## 📦 Comandos útiles

- Detener contenedores:

```bash
docker-compose down
```

---

## 🧾 Licencia

Apache License

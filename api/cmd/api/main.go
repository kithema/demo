package main

import (
	"log"
	"net/http"
	"os"
	"vendomatic-api/internal/config"
	"vendomatic-api/internal/db"
	"vendomatic-api/internal/routes"

	"github.com/joho/godotenv"
)

func main() {
	if err := godotenv.Load(); err != nil {
		log.Println("No .env file found, using environment variables")
	}

	cfg := config.LoadConfig()

	database, err := db.Connect(cfg)
	if err != nil {
		log.Fatal("Failed to connect to database:", err)
	}
	defer database.Close()

	if err := db.Ping(database); err != nil {
		log.Fatal("Failed to ping database:", err)
	}

	log.Println("Successfully connected to database")

	router := routes.SetupRoutes(database)

	port := os.Getenv("PORT")
	if port == "" {
		port = "8080"
	}

	log.Printf("Server starting on :%s", port)
	
	if err := http.ListenAndServe(":"+port, router); err != nil {
		log.Fatal("Failed to start server:", err)
	}
}
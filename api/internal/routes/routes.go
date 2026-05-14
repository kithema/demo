package routes

import (
	"database/sql"
	"encoding/json"
	"net/http"
	"vendomatic-api/internal/handlers"

	"github.com/go-chi/chi/v5"
	"github.com/go-chi/chi/v5/middleware"
)

func SetupRoutes(db *sql.DB) *chi.Mux {
	r := chi.NewRouter()

	// CORS middleware - разрешаем все запросы из браузера
	r.Use(corsMiddleware)
	
	r.Use(middleware.Logger)
	r.Use(middleware.Recoverer)
	r.Use(middleware.RequestID)
	r.Use(middleware.RealIP)

	r.Get("/health", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(map[string]string{"status": "ok"})
	})

	h := handlers.NewHandlers(db)

	// Vending Machines
	r.Route("/api/v1/machines", func(r chi.Router) {
		r.Get("/", h.GetAllVendingMachines)
		r.Post("/", h.CreateVendingMachine)
		r.Get("/{id}", h.GetVendingMachineByID)
		// r.Put("/{id}", h.UpdateVendingMachine)
	})

	// Products
	r.Route("/api/v1/products", func(r chi.Router) {
		r.Get("/", h.GetAllProducts)
		r.Post("/", h.CreateProduct)
		r.Get("/{id}", h.GetProductByID)
	})

	// Sales
	r.Route("/api/v1/sales", func(r chi.Router) {
		r.Get("/", h.GetAllSales)
		r.Post("/", h.CreateSale)
		r.Get("/{id}", h.GetSaleByID)
	})

	// Users
	r.Route("/api/v1/users", func(r chi.Router) {
		r.Get("/", h.GetAllUsers)
		r.Post("/", h.CreateUser)
		r.Get("/{id}", h.GetUserByID)
	})

	// Maintenance
	r.Route("/api/v1/maintenance", func(r chi.Router) {
		r.Get("/", h.GetAllMaintenanceRecords)
		r.Post("/", h.CreateMaintenance)
		r.Get("/{id}", h.GetMaintenanceByID)
	})

	
	r.Route("/api/v1/engineers", func(r chi.Router) {
		r.Get("/", h.GetAllEngineers)
		r.Post("/", h.CreateEngineer)
	})

	// Work Orders
	r.Route("/api/v1/work-orders", func(r chi.Router) {
		r.Get("/", h.GetAllWorkOrders)
		r.Post("/", h.CreateWorkOrder)
		r.Put("/{id}/status", h.UpdateOrderStatus)
		r.Put("/{id}/assign", h.AssignEngineer)
		r.Put("/{id}/date", h.UpdateOrderDate)
		r.Delete("/{id}", h.DeleteWorkOrder)
		r.Post("/apply", h.ApplySchedule)
	})

	return r
}

// CORS middleware
func corsMiddleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		// Разрешаем все источники
		w.Header().Set("Access-Control-Allow-Origin", "*")
		w.Header().Set("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS")
		w.Header().Set("Access-Control-Allow-Headers", "Content-Type, Authorization")
		w.Header().Set("Access-Control-Max-Age", "86400")

		// Обрабатываем preflight запросы
		if r.Method == "OPTIONS" {
			w.WriteHeader(http.StatusOK)
			return
		}

		next.ServeHTTP(w, r)
	})
}
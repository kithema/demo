package handlers

import (
	"database/sql"
	"encoding/json"
	"net/http"
	"strconv"
	"time"
	"vendomatic-api/internal/models"

	"io"
	"github.com/go-chi/chi/v5"
)

type Handlers struct {
	DB *sql.DB
}

func NewHandlers(db *sql.DB) *Handlers {
	return &Handlers{DB: db}
}


func (h *Handlers) GetAllVendingMachines(w http.ResponseWriter, r *http.Request) {
	query := `SELECT vm.machine_id, vm.serial_number, vm.inventory_number, vm.location, 
	          vm.model, vm.manufacturer, vm.manufacture_date, vm.commissioning_date, 
	          vm.last_verification_date, vm.verification_interval_months, vm.resource_hours,
	          vm.next_maintenance_date, vm.maintenance_time_hours, vm.status_id, s.status_name,
	          vm.country_id, c.country_name, vm.inventory_date, vm.last_verifier_employee, 
	          vm.total_income, vm.next_verification_date
	          FROM vending_machines vm
	          LEFT JOIN statuses s ON vm.status_id = s.status_id
	          LEFT JOIN countries c ON vm.country_id = c.country_id`

	rows, err := h.DB.Query(query)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	defer rows.Close()

	var machines []models.VendingMachine
	for rows.Next() {
		var m models.VendingMachine
		err := rows.Scan(
			&m.MachineID, &m.SerialNumber, &m.InventoryNumber, &m.Location,
			&m.Model, &m.Manufacturer, &m.ManufactureDate, &m.CommissioningDate,
			&m.LastVerificationDate, &m.VerificationIntervalMonths, &m.ResourceHours,
			&m.NextMaintenanceDate, &m.MaintenanceTimeHours, &m.StatusID, &m.StatusName,
			&m.CountryID, &m.CountryName, &m.InventoryDate, &m.LastVerifierEmployee,
			&m.TotalIncome, &m.NextVerificationDate,
		)
		if err != nil {
			respondWithError(w, http.StatusInternalServerError, err.Error())
			return
		}
		machines = append(machines, m)
	}

	respondWithJSON(w, http.StatusOK, machines)
}

func (h *Handlers) GetVendingMachineByID(w http.ResponseWriter, r *http.Request) {
	id, err := strconv.Atoi(chi.URLParam(r, "id"))
	if err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid ID")
		return
	}

	query := `SELECT vm.machine_id, vm.serial_number, vm.inventory_number, vm.location, 
	          vm.model, vm.manufacturer, vm.manufacture_date, vm.commissioning_date, 
	          vm.last_verification_date, vm.verification_interval_months, vm.resource_hours,
	          vm.next_maintenance_date, vm.maintenance_time_hours, vm.status_id, s.status_name,
	          vm.country_id, c.country_name, vm.inventory_date, vm.last_verifier_employee, 
	          vm.total_income, vm.next_verification_date
	          FROM vending_machines vm
	          LEFT JOIN statuses s ON vm.status_id = s.status_id
	          LEFT JOIN countries c ON vm.country_id = c.country_id
	          WHERE vm.machine_id = $1`

	var m models.VendingMachine
	err = h.DB.QueryRow(query, id).Scan(
		&m.MachineID, &m.SerialNumber, &m.InventoryNumber, &m.Location,
		&m.Model, &m.Manufacturer, &m.ManufactureDate, &m.CommissioningDate,
		&m.LastVerificationDate, &m.VerificationIntervalMonths, &m.ResourceHours,
		&m.NextMaintenanceDate, &m.MaintenanceTimeHours, &m.StatusID, &m.StatusName,
		&m.CountryID, &m.CountryName, &m.InventoryDate, &m.LastVerifierEmployee,
		&m.TotalIncome, &m.NextVerificationDate,
	)

	if err == sql.ErrNoRows {
		respondWithError(w, http.StatusNotFound, "Vending machine not found")
		return
	}
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	respondWithJSON(w, http.StatusOK, m)
}

func (h *Handlers) CreateVendingMachine(w http.ResponseWriter, r *http.Request) {
	var req models.CreateVendingMachineRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid request body: "+err.Error())
		return
	}

	var existingSerial string
	err := h.DB.QueryRow("SELECT serial_number FROM vending_machines WHERE serial_number = $1", req.SerialNumber).Scan(&existingSerial)
	if err == nil {
		respondWithError(w, http.StatusConflict, "ТА с таким серийным номером уже существует")
		return
	}

	err = h.DB.QueryRow("SELECT inventory_number FROM vending_machines WHERE inventory_number = $1", req.InventoryNumber).Scan(&existingSerial)
	if err == nil {
		respondWithError(w, http.StatusConflict, "ТА с таким инвентарным номером уже существует")
		return
	}

	currentDate := time.Now()
	if req.CommissioningDate.Before(req.ManufactureDate) || req.CommissioningDate.After(currentDate) {
		respondWithError(w, http.StatusBadRequest, "Дата ввода в эксплуатацию должна быть между датой изготовления и текущей датой")
		return
	}

	if req.LastVerificationDate != nil {
		if req.LastVerificationDate.Before(req.ManufactureDate) || req.LastVerificationDate.After(currentDate) {
			respondWithError(w, http.StatusBadRequest, "Дата последней поверки не может быть раньше даты изготовления и позже текущей даты")
			return
		}
	}

	if req.MaintenanceTimeHours != nil && (*req.MaintenanceTimeHours < 1 || *req.MaintenanceTimeHours > 20) {
		respondWithError(w, http.StatusBadRequest, "Время обслуживания должно быть от 1 до 20 часов")
		return
	}

	if req.ResourceHours != nil && *req.ResourceHours <= 0 {
		respondWithError(w, http.StatusBadRequest, "Ресурс ТА должен быть положительным числом")
		return
	}

	if req.NextMaintenanceDate != nil && req.NextMaintenanceDate.Before(currentDate) {
		respondWithError(w, http.StatusBadRequest, "Дата следующего обслуживания должна быть позже текущей даты")
		return
	}

	query := `INSERT INTO vending_machines 
	          (serial_number, inventory_number, location, model, manufacturer, 
	           manufacture_date, commissioning_date, last_verification_date, 
	           verification_interval_months, resource_hours, next_maintenance_date, 
	           maintenance_time_hours, status_id, country_id, inventory_date, 
	           last_verifier_employee, total_income)
	          VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15, $16, $17)
	          RETURNING machine_id`

	var id int
	err = h.DB.QueryRow(query,
		req.SerialNumber, req.InventoryNumber, req.Location, req.Model, req.Manufacturer,
		req.ManufactureDate, req.CommissioningDate, req.LastVerificationDate,
		req.VerificationIntervalMonths, req.ResourceHours, req.NextMaintenanceDate,
		req.MaintenanceTimeHours, req.StatusID, req.CountryID, req.InventoryDate,
		req.LastVerifierEmployee, 0,
	).Scan(&id)

	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	respondWithJSON(w, http.StatusCreated, map[string]int{"machine_id": id})
}



func (h *Handlers) GetAllProducts(w http.ResponseWriter, r *http.Request) {
	query := `SELECT product_id, name, description, price, min_stock, sales_trend FROM products`
	rows, err := h.DB.Query(query)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	defer rows.Close()

	var products []models.Product
	for rows.Next() {
		var p models.Product
		err := rows.Scan(&p.ProductID, &p.Name, &p.Description, &p.Price, &p.MinStock, &p.SalesTrend)
		if err != nil {
			respondWithError(w, http.StatusInternalServerError, err.Error())
			return
		}
		products = append(products, p)
	}

	respondWithJSON(w, http.StatusOK, products)
}

func (h *Handlers) GetProductByID(w http.ResponseWriter, r *http.Request) {
	id, err := strconv.Atoi(chi.URLParam(r, "id"))
	if err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid ID")
		return
	}

	query := `SELECT product_id, name, description, price, min_stock, sales_trend FROM products WHERE product_id = $1`
	var p models.Product
	err = h.DB.QueryRow(query, id).Scan(&p.ProductID, &p.Name, &p.Description, &p.Price, &p.MinStock, &p.SalesTrend)

	if err == sql.ErrNoRows {
		respondWithError(w, http.StatusNotFound, "Product not found")
		return
	}
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	respondWithJSON(w, http.StatusOK, p)
}

func (h *Handlers) CreateProduct(w http.ResponseWriter, r *http.Request) {
	var req models.CreateProductRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid request body")
		return
	}

	if req.Price <= 0 {
		respondWithError(w, http.StatusBadRequest, "Price must be greater than 0")
		return
	}

	minStock := 5
	if req.MinStock != nil {
		minStock = *req.MinStock
	}
	salesTrend := 0.0
	if req.SalesTrend != nil {
		salesTrend = *req.SalesTrend
	}

	query := `INSERT INTO products (name, description, price, min_stock, sales_trend)
	          VALUES ($1, $2, $3, $4, $5) RETURNING product_id`

	var id int
	err := h.DB.QueryRow(query, req.Name, req.Description, req.Price, minStock, salesTrend).Scan(&id)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	respondWithJSON(w, http.StatusCreated, map[string]int{"product_id": id})
}



func (h *Handlers) GetAllSales(w http.ResponseWriter, r *http.Request) {
	query := `SELECT sale_id, machine_id, product_id, quantity, amount, sale_datetime, payment_method FROM sales`
	rows, err := h.DB.Query(query)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	defer rows.Close()

	var sales []models.Sale
	for rows.Next() {
		var s models.Sale
		err := rows.Scan(&s.SaleID, &s.MachineID, &s.ProductID, &s.Quantity, &s.Amount, &s.SaleDatetime, &s.PaymentMethod)
		if err != nil {
			respondWithError(w, http.StatusInternalServerError, err.Error())
			return
		}
		sales = append(sales, s)
	}

	respondWithJSON(w, http.StatusOK, sales)
}

func (h *Handlers) GetSaleByID(w http.ResponseWriter, r *http.Request) {
	id, err := strconv.Atoi(chi.URLParam(r, "id"))
	if err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid ID")
		return
	}

	query := `SELECT sale_id, machine_id, product_id, quantity, amount, sale_datetime, payment_method FROM sales WHERE sale_id = $1`
	var s models.Sale
	err = h.DB.QueryRow(query, id).Scan(&s.SaleID, &s.MachineID, &s.ProductID, &s.Quantity, &s.Amount, &s.SaleDatetime, &s.PaymentMethod)

	if err == sql.ErrNoRows {
		respondWithError(w, http.StatusNotFound, "Sale not found")
		return
	}
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	respondWithJSON(w, http.StatusOK, s)
}

func (h *Handlers) CreateSale(w http.ResponseWriter, r *http.Request) {
	var req models.CreateSaleRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid request body")
		return
	}

	tx, err := h.DB.Begin()
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	defer tx.Rollback()

	query := `INSERT INTO sales (machine_id, product_id, quantity, amount, sale_datetime, payment_method)
	          VALUES ($1, $2, $3, $4, CURRENT_TIMESTAMP, $5) RETURNING sale_id`

	var id int
	err = tx.QueryRow(query, req.MachineID, req.ProductID, req.Quantity, req.Amount, req.PaymentMethod).Scan(&id)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	_, err = tx.Exec("UPDATE vending_machines SET total_income = COALESCE(total_income, 0) + $1 WHERE machine_id = $2", req.Amount, req.MachineID)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	if err := tx.Commit(); err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	respondWithJSON(w, http.StatusCreated, map[string]int{"sale_id": id})
}


func (h *Handlers) GetAllUsers(w http.ResponseWriter, r *http.Request) {
	query := `SELECT u.user_id, u.full_name, u.email, u.phone, u.role_id, r.role_name 
	          FROM users u
	          LEFT JOIN roles r ON u.role_id = r.role_id`
	rows, err := h.DB.Query(query)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	defer rows.Close()

	var users []models.User
	for rows.Next() {
		var u models.User
		err := rows.Scan(&u.UserID, &u.FullName, &u.Email, &u.Phone, &u.RoleID, &u.RoleName)
		if err != nil {
			respondWithError(w, http.StatusInternalServerError, err.Error())
			return
		}
		users = append(users, u)
	}

	respondWithJSON(w, http.StatusOK, users)
}

func (h *Handlers) GetUserByID(w http.ResponseWriter, r *http.Request) {
	id, err := strconv.Atoi(chi.URLParam(r, "id"))
	if err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid ID")
		return
	}

	query := `SELECT u.user_id, u.full_name, u.email, u.phone, u.role_id, r.role_name 
	          FROM users u
	          LEFT JOIN roles r ON u.role_id = r.role_id
	          WHERE u.user_id = $1`
	var u models.User
	err = h.DB.QueryRow(query, id).Scan(&u.UserID, &u.FullName, &u.Email, &u.Phone, &u.RoleID, &u.RoleName)

	if err == sql.ErrNoRows {
		respondWithError(w, http.StatusNotFound, "User not found")
		return
	}
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	respondWithJSON(w, http.StatusOK, u)
}

func (h *Handlers) CreateUser(w http.ResponseWriter, r *http.Request) {
	var req models.CreateUserRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid request body")
		return
	}

	query := `INSERT INTO users (full_name, email, phone, role_id) VALUES ($1, $2, $3, $4) RETURNING user_id`

	var id int
	err := h.DB.QueryRow(query, req.FullName, req.Email, req.Phone, req.RoleID).Scan(&id)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	respondWithJSON(w, http.StatusCreated, map[string]int{"user_id": id})
}

func (h *Handlers) GetAllMaintenanceRecords(w http.ResponseWriter, r *http.Request) {
	query := `SELECT maintenance_id, machine_id, maintenance_date, description, problems, executor FROM maintenance`
	rows, err := h.DB.Query(query)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	defer rows.Close()

	var records []models.Maintenance
	for rows.Next() {
		var m models.Maintenance
		err := rows.Scan(&m.MaintenanceID, &m.MachineID, &m.MaintenanceDate, &m.Description, &m.Problems, &m.Executor)
		if err != nil {
			respondWithError(w, http.StatusInternalServerError, err.Error())
			return
		}
		records = append(records, m)
	}

	respondWithJSON(w, http.StatusOK, records)
}

func (h *Handlers) GetMaintenanceByID(w http.ResponseWriter, r *http.Request) {
	id, err := strconv.Atoi(chi.URLParam(r, "id"))
	if err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid ID")
		return
	}

	query := `SELECT maintenance_id, machine_id, maintenance_date, description, problems, executor FROM maintenance WHERE maintenance_id = $1`
	var m models.Maintenance
	err = h.DB.QueryRow(query, id).Scan(&m.MaintenanceID, &m.MachineID, &m.MaintenanceDate, &m.Description, &m.Problems, &m.Executor)

	if err == sql.ErrNoRows {
		respondWithError(w, http.StatusNotFound, "Maintenance record not found")
		return
	}
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	respondWithJSON(w, http.StatusOK, m)
}

func (h *Handlers) CreateMaintenance(w http.ResponseWriter, r *http.Request) {
	var req models.CreateMaintenanceRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid request body")
		return
	}

	query := `INSERT INTO maintenance (machine_id, maintenance_date, description, problems, executor)
	          VALUES ($1, $2, $3, $4, $5) RETURNING maintenance_id`

	var id int
	err := h.DB.QueryRow(query, req.MachineID, req.MaintenanceDate, req.Description, req.Problems, req.Executor).Scan(&id)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	respondWithJSON(w, http.StatusCreated, map[string]int{"maintenance_id": id})
}

func respondWithJSON(w http.ResponseWriter, status int, payload interface{}) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	json.NewEncoder(w).Encode(payload)
}

func respondWithError(w http.ResponseWriter, status int, message string) {
	respondWithJSON(w, status, map[string]string{"error": message})
}


// ==================== ENGINEERS ====================

func (h *Handlers) GetAllEngineers(w http.ResponseWriter, r *http.Request) {
	query := `SELECT engineer_id, full_name, email, phone, max_tasks_per_week, is_active FROM engineers`
	rows, err := h.DB.Query(query)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	defer rows.Close()

	var engineers []models.Engineer
	for rows.Next() {
		var e models.Engineer
		err := rows.Scan(&e.EngineerID, &e.FullName, &e.Email, &e.Phone, &e.MaxTasksPerWeek, &e.IsActive)
		if err != nil {
			respondWithError(w, http.StatusInternalServerError, err.Error())
			return
		}
		engineers = append(engineers, e)
	}
	respondWithJSON(w, http.StatusOK, engineers)
}

func (h *Handlers) CreateEngineer(w http.ResponseWriter, r *http.Request) {
	var req models.Engineer
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondWithError(w, http.StatusBadRequest, err.Error())
		return
	}
	query := `INSERT INTO engineers (full_name, email, phone, max_tasks_per_week) VALUES ($1, $2, $3, $4) RETURNING engineer_id`
	var id int
	err := h.DB.QueryRow(query, req.FullName, req.Email, req.Phone, req.MaxTasksPerWeek).Scan(&id)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	respondWithJSON(w, http.StatusCreated, map[string]int{"engineer_id": id})
}

// ==================== WORK ORDERS ====================

func (h *Handlers) GetAllWorkOrders(w http.ResponseWriter, r *http.Request) {
	query := `SELECT order_id, machine_id, engineer_id, title, description, priority, status, scheduled_date, created_at FROM work_orders`
	rows, err := h.DB.Query(query)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	defer rows.Close()

	var orders []models.WorkOrder
	for rows.Next() {
		var o models.WorkOrder
		err := rows.Scan(&o.OrderID, &o.MachineID, &o.EngineerID, &o.Title, &o.Description, &o.Priority, &o.Status, &o.ScheduledDate, &o.CreatedAt)
		if err != nil {
			respondWithError(w, http.StatusInternalServerError, err.Error())
			return
		}
		orders = append(orders, o)
	}
	respondWithJSON(w, http.StatusOK, orders)
}

func (h *Handlers) CreateWorkOrder(w http.ResponseWriter, r *http.Request) {
	var req models.WorkOrder
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondWithError(w, http.StatusBadRequest, err.Error())
		return
	}
	query := `INSERT INTO work_orders (machine_id, title, description, priority, status, scheduled_date) VALUES ($1, $2, $3, $4, $5, $6) RETURNING order_id`
	var id int
	err := h.DB.QueryRow(query, req.MachineID, req.Title, req.Description, req.Priority, req.Status, req.ScheduledDate).Scan(&id)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	respondWithJSON(w, http.StatusCreated, map[string]int{"order_id": id})
}

func (h *Handlers) UpdateOrderStatus(w http.ResponseWriter, r *http.Request) {
	id, _ := strconv.Atoi(chi.URLParam(r, "id"))
	var status string
	body, _ := io.ReadAll(r.Body)
	status = string(body)
	_, err := h.DB.Exec("UPDATE work_orders SET status = $1 WHERE order_id = $2", status, id)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	respondWithJSON(w, http.StatusOK, map[string]string{"status": "updated"})
}

func (h *Handlers) AssignEngineer(w http.ResponseWriter, r *http.Request) {
	id, _ := strconv.Atoi(chi.URLParam(r, "id"))
	body, _ := io.ReadAll(r.Body)
	engineerID := string(body)
	var engID *int
	if engineerID != "" && engineerID != "null" {
		if eid, err := strconv.Atoi(engineerID); err == nil {
			engID = &eid
		}
	}
	_, err := h.DB.Exec("UPDATE work_orders SET engineer_id = $1 WHERE order_id = $2", engID, id)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	respondWithJSON(w, http.StatusOK, map[string]string{"status": "assigned"})
}

func (h *Handlers) UpdateOrderDate(w http.ResponseWriter, r *http.Request) {
	id, _ := strconv.Atoi(chi.URLParam(r, "id"))
	body, _ := io.ReadAll(r.Body)
	date := string(body)
	_, err := h.DB.Exec("UPDATE work_orders SET scheduled_date = $1 WHERE order_id = $2", date, id)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	respondWithJSON(w, http.StatusOK, map[string]string{"status": "updated"})
}

func (h *Handlers) DeleteWorkOrder(w http.ResponseWriter, r *http.Request) {
	id, _ := strconv.Atoi(chi.URLParam(r, "id"))
	_, err := h.DB.Exec("DELETE FROM work_orders WHERE order_id = $1", id)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	respondWithJSON(w, http.StatusOK, map[string]string{"status": "deleted"})
}

func (h *Handlers) ApplySchedule(w http.ResponseWriter, r *http.Request) {
	// Обновляем статусы заявок на "В работе"
	_, err := h.DB.Exec("UPDATE work_orders SET status = 'in_progress' WHERE status = 'new'")
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	// Обновляем статусы аппаратов
	_, err = h.DB.Exec(`UPDATE vending_machines SET status_id = (SELECT status_id FROM statuses WHERE status_name = 'В ремонте/на обслуживании') 
		WHERE machine_id IN (SELECT DISTINCT machine_id FROM work_orders WHERE status = 'in_progress')`)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	respondWithJSON(w, http.StatusOK, map[string]string{"status": "applied"})
} 

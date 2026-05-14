package models

import (
	"database/sql"
	"time"
)

type VendingMachine struct {
	MachineID                  int            `json:"machine_id"`
	SerialNumber               string         `json:"serial_number"`
	InventoryNumber            string         `json:"inventory_number"`
	Location                   string         `json:"location"`
	Model                      string         `json:"model"`
	Manufacturer               string         `json:"manufacturer"`
	ManufactureDate            time.Time      `json:"manufacture_date"`
	CommissioningDate          time.Time      `json:"commissioning_date"`
	LastVerificationDate       *time.Time     `json:"last_verification_date,omitempty"`
	VerificationIntervalMonths *int           `json:"verification_interval_months,omitempty"`
	ResourceHours              *int           `json:"resource_hours,omitempty"`
	NextMaintenanceDate        *time.Time     `json:"next_maintenance_date,omitempty"`
	MaintenanceTimeHours       *int           `json:"maintenance_time_hours,omitempty"`
	StatusID                   *int           `json:"status_id,omitempty"`
	StatusName                 sql.NullString `json:"status_name,omitempty"`  // Изменено на sql.NullString
	CountryID                  *int           `json:"country_id,omitempty"`
	CountryName                sql.NullString `json:"country_name,omitempty"` // Изменено на sql.NullString
	InventoryDate              *time.Time     `json:"inventory_date,omitempty"`
	LastVerifierEmployee       *string        `json:"last_verifier_employee,omitempty"`
	TotalIncome                float64        `json:"total_income"`
	NextVerificationDate       *time.Time     `json:"next_verification_date,omitempty"`
}

type Product struct {
	ProductID   int     `json:"product_id"`
	Name        string  `json:"name"`
	Description *string `json:"description,omitempty"`
	Price       float64 `json:"price"`
	MinStock    int     `json:"min_stock"`
	SalesTrend  float64 `json:"sales_trend"`
}

type Sale struct {
	SaleID        int       `json:"sale_id"`
	MachineID     *int      `json:"machine_id,omitempty"`
	ProductID     *int      `json:"product_id,omitempty"`
	Quantity      int       `json:"quantity"`
	Amount        float64   `json:"amount"`
	SaleDatetime  time.Time `json:"sale_datetime"`
	PaymentMethod *string   `json:"payment_method,omitempty"`
}

type User struct {
	UserID   int     `json:"user_id"`
	FullName string  `json:"full_name"`
	Email    *string `json:"email,omitempty"`
	Phone    *string `json:"phone,omitempty"`
	RoleID   int     `json:"role_id"`
	RoleName string  `json:"role_name,omitempty"`
}

type Maintenance struct {
	MaintenanceID   int        `json:"maintenance_id"`
	MachineID       *int       `json:"machine_id,omitempty"`
	MaintenanceDate time.Time  `json:"maintenance_date"`
	Description     string     `json:"description"`
	Problems        *string    `json:"problems,omitempty"`
	Executor        string     `json:"executor"`
}

type Country struct {
	CountryID   int    `json:"country_id"`
	CountryName string `json:"country_name"`
}

type Status struct {
	StatusID   int    `json:"status_id"`
	StatusName string `json:"status_name"`
}

type Role struct {
	RoleID   int    `json:"role_id"`
	RoleName string `json:"role_name"`
}

type VendingType struct {
	TypeID   int    `json:"type_id"`
	TypeName string `json:"type_name"`
}

type CreateVendingMachineRequest struct {
	SerialNumber               string     `json:"serial_number"`
	InventoryNumber            string     `json:"inventory_number"`
	Location                   string     `json:"location"`
	Model                      string     `json:"model"`
	Manufacturer               string     `json:"manufacturer"`
	ManufactureDate            time.Time  `json:"manufacture_date"`
	CommissioningDate          time.Time  `json:"commissioning_date"`
	LastVerificationDate       *time.Time `json:"last_verification_date,omitempty"`
	VerificationIntervalMonths *int       `json:"verification_interval_months,omitempty"`
	ResourceHours              *int       `json:"resource_hours,omitempty"`
	NextMaintenanceDate        *time.Time `json:"next_maintenance_date,omitempty"`
	MaintenanceTimeHours       *int       `json:"maintenance_time_hours,omitempty"`
	StatusID                   *int       `json:"status_id,omitempty"`
	CountryID                  *int       `json:"country_id,omitempty"`
	InventoryDate              *time.Time `json:"inventory_date,omitempty"`
	LastVerifierEmployee       *string    `json:"last_verifier_employee,omitempty"`
}

type CreateProductRequest struct {
	Name        string   `json:"name"`
	Description *string  `json:"description,omitempty"`
	Price       float64  `json:"price"`
	MinStock    *int     `json:"min_stock,omitempty"`
	SalesTrend  *float64 `json:"sales_trend,omitempty"`
}

type CreateSaleRequest struct {
	MachineID     int     `json:"machine_id"`
	ProductID     int     `json:"product_id"`
	Quantity      int     `json:"quantity"`
	Amount        float64 `json:"amount"`
	PaymentMethod string  `json:"payment_method"`
}

type CreateUserRequest struct {
	FullName string  `json:"full_name"`
	Email    *string `json:"email,omitempty"`
	Phone    *string `json:"phone,omitempty"`
	RoleID   int     `json:"role_id"`
}

type CreateMaintenanceRequest struct {
	MachineID       int       `json:"machine_id"`
	MaintenanceDate time.Time `json:"maintenance_date"`
	Description     string    `json:"description"`
	Problems        *string   `json:"problems,omitempty"`
	Executor        string    `json:"executor"`
}


// Engineer - сотрудник
type Engineer struct {
	EngineerID      int     `json:"engineer_id"`
	FullName        string  `json:"full_name"`
	Email           *string `json:"email,omitempty"`
	Phone           *string `json:"phone,omitempty"`
	MaxTasksPerWeek int     `json:"max_tasks_per_week"`
	IsActive        bool    `json:"is_active"`
}

// WorkOrder - заявка на обслуживание
type WorkOrder struct {
	OrderID      int        `json:"order_id"`
	MachineID    int        `json:"machine_id"`
	EngineerID   *int       `json:"engineer_id,omitempty"`
	Title        string     `json:"title"`
	Description  *string    `json:"description,omitempty"`
	Priority     string     `json:"priority"`
	Status       string     `json:"status"`
	ScheduledDate *string   `json:"scheduled_date,omitempty"`
	CreatedAt    time.Time  `json:"created_at"`
}
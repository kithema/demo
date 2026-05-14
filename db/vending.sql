
\restrict 8VX8Wz77Lb7esr21eGogpPy5aenuzGXq47NcpcaKp82yeJ3g2nwitsvk7tNrAbb

-- Dumped from database version 18.1
-- Dumped by pg_dump version 18.1


SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 245 (class 1255 OID 24936)
-- Name: calculate_next_verification(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.calculate_next_verification() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF NEW.last_verification_date IS NOT NULL 
       AND NEW.verification_interval_months IS NOT NULL THEN
        NEW.next_verification_date := NEW.last_verification_date 
                                    + INTERVAL '1 month' * NEW.verification_interval_months;
    ELSE
        NEW.next_verification_date := NULL;
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.calculate_next_verification() OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 220 (class 1259 OID 24854)
-- Name: countries; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.countries (
    country_id integer NOT NULL,
    country_name character varying(100) NOT NULL
);


ALTER TABLE public.countries OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 24853)
-- Name: countries_country_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.countries_country_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.countries_country_id_seq OWNER TO postgres;

--
-- TOC entry 5184 (class 0 OID 0)
-- Dependencies: 219
-- Name: countries_country_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.countries_country_id_seq OWNED BY public.countries.country_id;


--
-- TOC entry 240 (class 1259 OID 25073)
-- Name: engineer_models; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.engineer_models (
    id integer NOT NULL,
    engineer_id integer,
    model_name character varying(100) NOT NULL
);


ALTER TABLE public.engineer_models OWNER TO postgres;

--
-- TOC entry 239 (class 1259 OID 25072)
-- Name: engineer_models_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.engineer_models_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.engineer_models_id_seq OWNER TO postgres;

--
-- TOC entry 5185 (class 0 OID 0)
-- Dependencies: 239
-- Name: engineer_models_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.engineer_models_id_seq OWNED BY public.engineer_models.id;


--
-- TOC entry 238 (class 1259 OID 25062)
-- Name: engineers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.engineers (
    engineer_id integer NOT NULL,
    full_name character varying(200) NOT NULL,
    email character varying(150),
    phone character varying(50),
    max_tasks_per_week integer DEFAULT 15,
    is_active boolean DEFAULT true
);


ALTER TABLE public.engineers OWNER TO postgres;

--
-- TOC entry 237 (class 1259 OID 25061)
-- Name: engineers_engineer_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.engineers_engineer_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.engineers_engineer_id_seq OWNER TO postgres;

--
-- TOC entry 5186 (class 0 OID 0)
-- Dependencies: 237
-- Name: engineers_engineer_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.engineers_engineer_id_seq OWNED BY public.engineers.engineer_id;


--
-- TOC entry 244 (class 1259 OID 25111)
-- Name: machine_import_temp; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.machine_import_temp (
    id integer NOT NULL,
    serial_number character varying(100),
    inventory_number character varying(100),
    location text,
    model character varying(100),
    manufacturer character varying(100),
    manufacture_date date,
    commissioning_date date,
    status_name character varying(50),
    country_name character varying(100),
    import_status character varying(20),
    error_message text
);


ALTER TABLE public.machine_import_temp OWNER TO postgres;

--
-- TOC entry 243 (class 1259 OID 25110)
-- Name: machine_import_temp_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.machine_import_temp_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.machine_import_temp_id_seq OWNER TO postgres;

--
-- TOC entry 5187 (class 0 OID 0)
-- Dependencies: 243
-- Name: machine_import_temp_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.machine_import_temp_id_seq OWNED BY public.machine_import_temp.id;


--
-- TOC entry 236 (class 1259 OID 24999)
-- Name: maintenance; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.maintenance (
    maintenance_id integer NOT NULL,
    machine_id integer,
    maintenance_date date NOT NULL,
    description text NOT NULL,
    problems text,
    executor character varying(200) NOT NULL
);


ALTER TABLE public.maintenance OWNER TO postgres;

--
-- TOC entry 235 (class 1259 OID 24998)
-- Name: maintenance_maintenance_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.maintenance_maintenance_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.maintenance_maintenance_id_seq OWNER TO postgres;

--
-- TOC entry 5188 (class 0 OID 0)
-- Dependencies: 235
-- Name: maintenance_maintenance_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.maintenance_maintenance_id_seq OWNED BY public.maintenance.maintenance_id;


--
-- TOC entry 230 (class 1259 OID 24939)
-- Name: products; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.products (
    product_id integer NOT NULL,
    name character varying(200) NOT NULL,
    description text,
    price numeric(10,2) NOT NULL,
    min_stock integer DEFAULT 5,
    sales_trend numeric(10,2) DEFAULT 0.0,
    CONSTRAINT products_min_stock_check CHECK ((min_stock >= 0)),
    CONSTRAINT products_price_check CHECK ((price > (0)::numeric))
);


ALTER TABLE public.products OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 24938)
-- Name: products_product_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.products_product_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.products_product_id_seq OWNER TO postgres;

--
-- TOC entry 5189 (class 0 OID 0)
-- Dependencies: 229
-- Name: products_product_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.products_product_id_seq OWNED BY public.products.product_id;


--
-- TOC entry 224 (class 1259 OID 24876)
-- Name: roles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.roles (
    role_id integer NOT NULL,
    role_name character varying(50) NOT NULL
);


ALTER TABLE public.roles OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 24875)
-- Name: roles_role_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.roles_role_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.roles_role_id_seq OWNER TO postgres;

--
-- TOC entry 5190 (class 0 OID 0)
-- Dependencies: 223
-- Name: roles_role_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.roles_role_id_seq OWNED BY public.roles.role_id;


--
-- TOC entry 234 (class 1259 OID 24974)
-- Name: sales; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sales (
    sale_id bigint NOT NULL,
    machine_id integer,
    product_id integer,
    quantity integer NOT NULL,
    amount numeric(12,2) NOT NULL,
    sale_datetime timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    payment_method character varying(20),
    CONSTRAINT sales_amount_check CHECK ((amount > (0)::numeric)),
    CONSTRAINT sales_payment_method_check CHECK (((payment_method)::text = ANY ((ARRAY['cash'::character varying, 'card'::character varying, 'qr'::character varying])::text[]))),
    CONSTRAINT sales_quantity_check CHECK ((quantity > 0))
);


ALTER TABLE public.sales OWNER TO postgres;

--
-- TOC entry 233 (class 1259 OID 24973)
-- Name: sales_sale_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.sales_sale_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.sales_sale_id_seq OWNER TO postgres;

--
-- TOC entry 5191 (class 0 OID 0)
-- Dependencies: 233
-- Name: sales_sale_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.sales_sale_id_seq OWNED BY public.sales.sale_id;


--
-- TOC entry 222 (class 1259 OID 24865)
-- Name: statuses; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.statuses (
    status_id integer NOT NULL,
    status_name character varying(50) NOT NULL
);


ALTER TABLE public.statuses OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 24864)
-- Name: statuses_status_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.statuses_status_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.statuses_status_id_seq OWNER TO postgres;

--
-- TOC entry 5192 (class 0 OID 0)
-- Dependencies: 221
-- Name: statuses_status_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.statuses_status_id_seq OWNED BY public.statuses.status_id;


--
-- TOC entry 232 (class 1259 OID 24955)
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    user_id integer NOT NULL,
    full_name character varying(200) NOT NULL,
    email character varying(150),
    phone character varying(50),
    role_id integer NOT NULL
);


ALTER TABLE public.users OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 24954)
-- Name: users_user_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.users_user_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_user_id_seq OWNER TO postgres;

--
-- TOC entry 5193 (class 0 OID 0)
-- Dependencies: 231
-- Name: users_user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.users_user_id_seq OWNED BY public.users.user_id;


--
-- TOC entry 228 (class 1259 OID 24898)
-- Name: vending_machines; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.vending_machines (
    machine_id integer NOT NULL,
    serial_number character varying(100) NOT NULL,
    inventory_number character varying(100) NOT NULL,
    location text NOT NULL,
    model character varying(100) NOT NULL,
    manufacturer character varying(100) NOT NULL,
    manufacture_date date NOT NULL,
    commissioning_date date NOT NULL,
    last_verification_date date,
    verification_interval_months integer,
    resource_hours integer,
    next_maintenance_date date,
    maintenance_time_hours integer,
    status_id integer,
    country_id integer,
    inventory_date date,
    last_verifier_employee character varying(200),
    total_income numeric(15,2) DEFAULT 0.00,
    next_verification_date date,
    CONSTRAINT chk_commissioning_date CHECK (((commissioning_date >= manufacture_date) AND (commissioning_date <= CURRENT_DATE))),
    CONSTRAINT chk_inventory_date CHECK (((inventory_date IS NULL) OR ((inventory_date >= manufacture_date) AND (inventory_date <= CURRENT_DATE)))),
    CONSTRAINT chk_last_verification CHECK (((last_verification_date IS NULL) OR ((last_verification_date >= manufacture_date) AND (last_verification_date <= CURRENT_DATE)))),
    CONSTRAINT chk_next_maintenance CHECK (((next_maintenance_date IS NULL) OR (next_maintenance_date > CURRENT_DATE))),
    CONSTRAINT vending_machines_maintenance_time_hours_check CHECK (((maintenance_time_hours >= 1) AND (maintenance_time_hours <= 20))),
    CONSTRAINT vending_machines_resource_hours_check CHECK ((resource_hours > 0)),
    CONSTRAINT vending_machines_verification_interval_months_check CHECK ((verification_interval_months > 0))
);


ALTER TABLE public.vending_machines OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 24897)
-- Name: vending_machines_machine_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.vending_machines_machine_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.vending_machines_machine_id_seq OWNER TO postgres;

--
-- TOC entry 5194 (class 0 OID 0)
-- Dependencies: 227
-- Name: vending_machines_machine_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.vending_machines_machine_id_seq OWNED BY public.vending_machines.machine_id;


--
-- TOC entry 226 (class 1259 OID 24887)
-- Name: vending_types; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.vending_types (
    type_id integer NOT NULL,
    type_name character varying(50) NOT NULL
);


ALTER TABLE public.vending_types OWNER TO postgres;

--
-- TOC entry 225 (class 1259 OID 24886)
-- Name: vending_types_type_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.vending_types_type_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.vending_types_type_id_seq OWNER TO postgres;

--
-- TOC entry 5195 (class 0 OID 0)
-- Dependencies: 225
-- Name: vending_types_type_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.vending_types_type_id_seq OWNED BY public.vending_types.type_id;


--
-- TOC entry 242 (class 1259 OID 25087)
-- Name: work_orders; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.work_orders (
    order_id integer NOT NULL,
    machine_id integer,
    engineer_id integer,
    title character varying(200) NOT NULL,
    description text,
    priority character varying(20) DEFAULT 'normal'::character varying,
    status character varying(20) DEFAULT 'new'::character varying,
    scheduled_date date,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    completed_at timestamp without time zone
);


ALTER TABLE public.work_orders OWNER TO postgres;

--
-- TOC entry 241 (class 1259 OID 25086)
-- Name: work_orders_order_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.work_orders_order_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.work_orders_order_id_seq OWNER TO postgres;

--
-- TOC entry 5196 (class 0 OID 0)
-- Dependencies: 241
-- Name: work_orders_order_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.work_orders_order_id_seq OWNED BY public.work_orders.order_id;


--
-- TOC entry 4917 (class 2604 OID 24857)
-- Name: countries country_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.countries ALTER COLUMN country_id SET DEFAULT nextval('public.countries_country_id_seq'::regclass);


--
-- TOC entry 4933 (class 2604 OID 25076)
-- Name: engineer_models id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.engineer_models ALTER COLUMN id SET DEFAULT nextval('public.engineer_models_id_seq'::regclass);


--
-- TOC entry 4930 (class 2604 OID 25065)
-- Name: engineers engineer_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.engineers ALTER COLUMN engineer_id SET DEFAULT nextval('public.engineers_engineer_id_seq'::regclass);


--
-- TOC entry 4938 (class 2604 OID 25114)
-- Name: machine_import_temp id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.machine_import_temp ALTER COLUMN id SET DEFAULT nextval('public.machine_import_temp_id_seq'::regclass);


--
-- TOC entry 4929 (class 2604 OID 25002)
-- Name: maintenance maintenance_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.maintenance ALTER COLUMN maintenance_id SET DEFAULT nextval('public.maintenance_maintenance_id_seq'::regclass);


--
-- TOC entry 4923 (class 2604 OID 24942)
-- Name: products product_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.products ALTER COLUMN product_id SET DEFAULT nextval('public.products_product_id_seq'::regclass);


--
-- TOC entry 4919 (class 2604 OID 24879)
-- Name: roles role_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.roles ALTER COLUMN role_id SET DEFAULT nextval('public.roles_role_id_seq'::regclass);


--
-- TOC entry 4927 (class 2604 OID 24977)
-- Name: sales sale_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sales ALTER COLUMN sale_id SET DEFAULT nextval('public.sales_sale_id_seq'::regclass);


--
-- TOC entry 4918 (class 2604 OID 24868)
-- Name: statuses status_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.statuses ALTER COLUMN status_id SET DEFAULT nextval('public.statuses_status_id_seq'::regclass);


--
-- TOC entry 4926 (class 2604 OID 24958)
-- Name: users user_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users ALTER COLUMN user_id SET DEFAULT nextval('public.users_user_id_seq'::regclass);


--
-- TOC entry 4921 (class 2604 OID 24901)
-- Name: vending_machines machine_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.vending_machines ALTER COLUMN machine_id SET DEFAULT nextval('public.vending_machines_machine_id_seq'::regclass);


--
-- TOC entry 4920 (class 2604 OID 24890)
-- Name: vending_types type_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.vending_types ALTER COLUMN type_id SET DEFAULT nextval('public.vending_types_type_id_seq'::regclass);


--
-- TOC entry 4934 (class 2604 OID 25090)
-- Name: work_orders order_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.work_orders ALTER COLUMN order_id SET DEFAULT nextval('public.work_orders_order_id_seq'::regclass);


--
-- TOC entry 5154 (class 0 OID 24854)
-- Dependencies: 220
-- Data for Name: countries; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.countries (country_id, country_name) FROM stdin;
1	Россия
2	Китай
3	Германия
4	США
\.


--
-- TOC entry 5174 (class 0 OID 25073)
-- Dependencies: 240
-- Data for Name: engineer_models; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.engineer_models (id, engineer_id, model_name) FROM stdin;
\.


--
-- TOC entry 5172 (class 0 OID 25062)
-- Dependencies: 238
-- Data for Name: engineers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.engineers (engineer_id, full_name, email, phone, max_tasks_per_week, is_active) FROM stdin;
\.


--
-- TOC entry 5178 (class 0 OID 25111)
-- Dependencies: 244
-- Data for Name: machine_import_temp; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.machine_import_temp (id, serial_number, inventory_number, location, model, manufacturer, manufacture_date, commissioning_date, status_name, country_name, import_status, error_message) FROM stdin;
\.


--
-- TOC entry 5170 (class 0 OID 24999)
-- Dependencies: 236
-- Data for Name: maintenance; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.maintenance (maintenance_id, machine_id, maintenance_date, description, problems, executor) FROM stdin;
1	3	2026-01-22	Плановое ТО: очистка камер, проверка датчиков, смазка механизмов	Загрязнение датчиков наличия товара, ложные срабатывания	1
2	2	2026-01-21	Пополнение запасов: загружены 50 шт. воды, 30 шт. снеков	Низкий уровень запасов: осталось 5 бутылок воды, 2 батончика	2
3	1	2026-01-20	Замена вышедшего из строя дисплея управления	Экран не реагирует на касания, вероятный обрыв шлейфа	3
4	7	2026-01-19	Чистка системы подачи напитков, промывка трубок	Протечка в системе подачи воды, износ уплотнителя	4
5	6	2026-01-18	Обновление ПО до версии 2.1.5, перезагрузка системы	Ошибка связи с платёжным терминалом (код 105)	5
6	5	2026-01-17	Регулировка механизма выдачи товара, калибровка сенсоров	Заедание механизма выдачи, скопление мусора в лотке	6
7	9	2026-01-16	Замена аккумулятора резервного питания	Разряд резервного аккумулятора ниже 20 %	7
8	10	2026-01-15	Пополнение монетного механизма, инкассация наличных	Некорректное отображение цен на экране (сбой кэша)	8
9	8	2026-01-14	Установка нового модуля безналичной оплаты	Повреждение кабеля питания, оголение контактов	9
10	4	2026-01-13	Проверка герметичности корпуса, устранение зазоров	Повышенный шум вентилятора охлаждения, износ подшипников	10
\.


--
-- TOC entry 5164 (class 0 OID 24939)
-- Dependencies: 230
-- Data for Name: products; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.products (product_id, name, description, price, min_stock, sales_trend) FROM stdin;
1	Кофе «Эспрессо»	Эспрессо из 100% арабики, без добавок. Объём: 250 мл	120.00	18	3.50
2	Чипсы «Сыр & Лук»	Картофельные чипсы с ароматом сыра и лука. Без ГМО	95.00	25	2.10
3	Вода минеральная негазированная	Природная минеральная вода, низкоминерализованная. Без газа	60.00	40	4.80
4	Шоколадный батончик «Ореховый восторг»	Молочный шоколад с цельным фундуком и карамельной начинкой	85.00	30	1.90
5	Газированный напиток «Кола»	Газированный напиток со вкусом колы, с кофеином	75.00	22	2.70
6	Смесь орехов «Классика»	Смесь миндаля, фундука и грецкого ореха, слегка подсоленная	150.00	15	1.20
7	Леденцы «Мятные»	Мятные леденцы без сахара, с натуральным ароматизатором	45.00	50	5.30
8	Попкорн «Сливочный»	Воздушный попкорн со сливочным маслом и солью	70.00	28	1.80
9	Энергетический напиток «Turbo»	Энергетический напиток с таурином, кофеином и витаминами группы B	130.00	12	2.40
10	Чипсы Lays	Картофельные чипсы со вкусом сыра 150г	150.00	10	5.50
11	Чипсы Lays	Картофельные чипсы со вкусом сыра 150г	150.00	10	5.50
\.


--
-- TOC entry 5158 (class 0 OID 24876)
-- Dependencies: 224
-- Data for Name: roles; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.roles (role_id, role_name) FROM stdin;
1	Администратор
2	Оператор
3	Сервисный инженер
\.


--
-- TOC entry 5168 (class 0 OID 24974)
-- Dependencies: 234
-- Data for Name: sales; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.sales (sale_id, machine_id, product_id, quantity, amount, sale_datetime, payment_method) FROM stdin;
1	2	9	1	120.00	2026-01-22 08:15:30+03	card
2	5	2	3	285.00	2026-01-22 10:45:12+03	cash
3	9	6	2	120.00	2026-01-22 12:30:45+03	qr
4	8	5	1	85.00	2026-01-22 14:20:05+03	card
5	6	7	4	300.00	2026-01-22 16:55:22+03	cash
6	1	1	1	150.00	2026-01-22 18:03:17+03	qr
7	3	4	5	225.00	2026-01-22 19:40:50+03	card
8	10	2	2	140.00	2026-01-22 21:10:33+03	cash
9	4	8	1	130.00	2026-01-22 22:50:47+03	qr
10	7	7	3	165.00	2026-01-22 23:59:01+03	card
\.


--
-- TOC entry 5156 (class 0 OID 24865)
-- Dependencies: 222
-- Data for Name: statuses; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.statuses (status_id, status_name) FROM stdin;
1	Работает
2	Вышел из строя
3	В ремонте/на обслуживании
\.


--
-- TOC entry 5166 (class 0 OID 24955)
-- Dependencies: 232
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (user_id, full_name, email, phone, role_id) FROM stdin;
1	Иванов Алексей Петрович	alex.ivanov@example.com	+7 916 123-45-67	1
2	Петрова Мария Ивановна	maria.petrova@mail.ru	+7 903 234-56-78	2
3	Сидоров Дмитрий Викторович	dmitry.sidorov@yandex.ru	+7 926 345-67-89	2
4	Кузнецова Елена Павловна	elena.kuznetsova@gmail.com	+7 915 456-78-90	2
5	Морозов Роман Николаевич	roman.morozov@company.org	+7 909 567-89-01	1
6	Волкова Татьяна Леонидовна	tatyana.volkova@example.net	+7 925 678-90-12	2
7	Алексеев Сергей Михайлович	sergey.alekseev@biz.ru	+7 910 789-01-23	2
8	Никитина Ольга Александровна	olga.nikitina@proton.me	+7 905 890-12-34	2
9	Фёдоров Игорь Борисович	igor.fedorov@outlook.com	+7 927 901-23-45	1
10	Григорьева Наталья Константиновна	natalia.grigorieva@mail.com	+7 901 012-34-56	1
\.


--
-- TOC entry 5162 (class 0 OID 24898)
-- Dependencies: 228
-- Data for Name: vending_machines; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.vending_machines (machine_id, serial_number, inventory_number, location, model, manufacturer, manufacture_date, commissioning_date, last_verification_date, verification_interval_months, resource_hours, next_maintenance_date, maintenance_time_hours, status_id, country_id, inventory_date, last_verifier_employee, total_income, next_verification_date) FROM stdin;
2	SN987654321	INV-2025-002	Московская обл., г. Химки, ул. Московская, д. 15, офис 301.	CoffeeMaster Pro 500	АО «КофеМаш»	2025-05-04	2025-05-09	2025-05-24	12	1800	\N	8	2	2	2025-07-15	Петрова М. И.	1250000.00	2026-05-24
3	VCX200-001	INV-2025-003	г. Казань, ул. Баумана, д. 20, кафетерий	SnackVend S-300	ЗАО «СнекВенд»	2025-05-29	2025-05-31	2025-06-14	24	1801	\N	12	3	3	2025-08-10	Сидоров Д. В.	1250000.00	2027-06-14
4	CM500-PRO-002	INV-2025-004	г. Екатеринбург, ул. Ленина, д. 50, холл бизнес-центра.	AquaVend Water 2025	ООО «АкваВенд»	2025-06-19	2025-06-24	2025-07-30	18	1802	\N	6	1	\N	2025-08-10	Кузнецова Е. П.	1250000.00	2027-01-30
5	SV300-SN003	INV-2025-005	г. Новосибирск, Красный пр., д. 100, университетский кампус.	VendoTech Elite 400	ООО «ТехноВенд»	2025-08-04	2025-08-09	2025-08-10	36	1803	\N	16	3	4	2025-09-10	Морозов Р. Н.	1250000.00	2028-08-10
6	AW2025-004	INV-2025-006	г. Сочи, Курортный пр., д. 70, отель «Морская звезда», лобби.	QuickBite Mini 100	ИП «МиниВенд»	2025-08-14	2025-08-19	2025-09-24	12	1804	\N	10	2	\N	2025-10-11	Волкова Т. Л.	1250000.00	2026-09-24
8	QB100-MIN-006	INV-2025-008	г. Самара, ул. Молодогвардейская, д. 120, ТЦ «Мега».	FreshFood Vend 700	АО «ФрэшФудВенд»	2025-10-07	2025-10-09	2025-10-25	24	1806	\N	18	3	\N	2025-11-01	Никитина О. А.	1250000.00	2027-10-25
9	HDS600-007	INV-2025-009	г. Ростов-на-Дону, ул. Садовая, д. 80, административное здание.	IceCream Vend 250	ООО «АйсВенд»	2025-10-23	2025-10-30	2025-11-01	18	1807	\N	7	2	\N	2025-11-04	Фёдоров И. Б.	1250000.00	2027-05-01
10	FF700-VND-008	INV-2025-010	г. Владивосток, ул. Светланская, д. 60, морской вокзал.	Print&Go Kiosk 150	ЗАО «ПринтВенд»	2025-11-03	2025-11-09	2025-11-14	12	1808	\N	14	1	\N	2025-11-13	Григорьева Н. К.	1250000.00	2026-11-14
7	VT400-ELT-005	INV-2025-007	г. Нижний Новгород, ул. Большая Покровская, д. 40, торговый пассаж.	HotDrink Station 600	ООО «ГорячийНапиток»	2025-09-21	2025-09-24	2025-10-31	6	1805	\N	3	3	\N	2025-10-11	Алексеев С. М.	1250000.00	2026-04-30
1	SC123456789	INV-2025-001	г. Санкт-Петербург, Невский пр., д. 50, ТЦ «Галерея», 2-й этаж.	VendCore X-200	ООО «ВендТех»	2025-03-20	2025-03-29	2025-05-04	6	2500	\N	4	3	1	2025-07-09	Иванов А. С.	1250000.00	2025-11-04
\.


--
-- TOC entry 5160 (class 0 OID 24887)
-- Dependencies: 226
-- Data for Name: vending_types; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.vending_types (type_id, type_name) FROM stdin;
1	card
2	cash
3	both
\.


--
-- TOC entry 5176 (class 0 OID 25087)
-- Dependencies: 242
-- Data for Name: work_orders; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.work_orders (order_id, machine_id, engineer_id, title, description, priority, status, scheduled_date, created_at, completed_at) FROM stdin;
2	1	\N	ыфвф	ыфв	high	in_progress	2026-04-28	2026-04-27 01:21:40.426735	\N
3	1	\N	фывыв	фывфывф	emergency	in_progress	2026-04-28	2026-04-27 01:22:14.105352	\N
1	7	\N	Ужас	полный страх	high	completed	2026-04-28	2026-04-27 01:17:26.942813	\N
\.


--
-- TOC entry 5197 (class 0 OID 0)
-- Dependencies: 219
-- Name: countries_country_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.countries_country_id_seq', 4, true);


--
-- TOC entry 5198 (class 0 OID 0)
-- Dependencies: 239
-- Name: engineer_models_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.engineer_models_id_seq', 1, false);


--
-- TOC entry 5199 (class 0 OID 0)
-- Dependencies: 237
-- Name: engineers_engineer_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.engineers_engineer_id_seq', 1, false);


--
-- TOC entry 5200 (class 0 OID 0)
-- Dependencies: 243
-- Name: machine_import_temp_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.machine_import_temp_id_seq', 1, false);


--
-- TOC entry 5201 (class 0 OID 0)
-- Dependencies: 235
-- Name: maintenance_maintenance_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.maintenance_maintenance_id_seq', 10, true);


--
-- TOC entry 5202 (class 0 OID 0)
-- Dependencies: 229
-- Name: products_product_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.products_product_id_seq', 11, true);


--
-- TOC entry 5203 (class 0 OID 0)
-- Dependencies: 223
-- Name: roles_role_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.roles_role_id_seq', 3, true);


--
-- TOC entry 5204 (class 0 OID 0)
-- Dependencies: 233
-- Name: sales_sale_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.sales_sale_id_seq', 10, true);


--
-- TOC entry 5205 (class 0 OID 0)
-- Dependencies: 221
-- Name: statuses_status_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.statuses_status_id_seq', 3, true);


--
-- TOC entry 5206 (class 0 OID 0)
-- Dependencies: 231
-- Name: users_user_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.users_user_id_seq', 10, true);


--
-- TOC entry 5207 (class 0 OID 0)
-- Dependencies: 227
-- Name: vending_machines_machine_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.vending_machines_machine_id_seq', 10, true);


--
-- TOC entry 5208 (class 0 OID 0)
-- Dependencies: 225
-- Name: vending_types_type_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.vending_types_type_id_seq', 3, true);


--
-- TOC entry 5209 (class 0 OID 0)
-- Dependencies: 241
-- Name: work_orders_order_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.work_orders_order_id_seq', 3, true);


--
-- TOC entry 4952 (class 2606 OID 24863)
-- Name: countries countries_country_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.countries
    ADD CONSTRAINT countries_country_name_key UNIQUE (country_name);


--
-- TOC entry 4954 (class 2606 OID 24861)
-- Name: countries countries_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.countries
    ADD CONSTRAINT countries_pkey PRIMARY KEY (country_id);


--
-- TOC entry 4991 (class 2606 OID 25080)
-- Name: engineer_models engineer_models_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.engineer_models
    ADD CONSTRAINT engineer_models_pkey PRIMARY KEY (id);


--
-- TOC entry 4989 (class 2606 OID 25071)
-- Name: engineers engineers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.engineers
    ADD CONSTRAINT engineers_pkey PRIMARY KEY (engineer_id);


--
-- TOC entry 4995 (class 2606 OID 25119)
-- Name: machine_import_temp machine_import_temp_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.machine_import_temp
    ADD CONSTRAINT machine_import_temp_pkey PRIMARY KEY (id);


--
-- TOC entry 4987 (class 2606 OID 25010)
-- Name: maintenance maintenance_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.maintenance
    ADD CONSTRAINT maintenance_pkey PRIMARY KEY (maintenance_id);


--
-- TOC entry 4974 (class 2606 OID 24953)
-- Name: products products_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_pkey PRIMARY KEY (product_id);


--
-- TOC entry 4960 (class 2606 OID 24883)
-- Name: roles roles_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT roles_pkey PRIMARY KEY (role_id);


--
-- TOC entry 4962 (class 2606 OID 24885)
-- Name: roles roles_role_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT roles_role_name_key UNIQUE (role_name);


--
-- TOC entry 4984 (class 2606 OID 24987)
-- Name: sales sales_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sales
    ADD CONSTRAINT sales_pkey PRIMARY KEY (sale_id);


--
-- TOC entry 4956 (class 2606 OID 24872)
-- Name: statuses statuses_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.statuses
    ADD CONSTRAINT statuses_pkey PRIMARY KEY (status_id);


--
-- TOC entry 4958 (class 2606 OID 24874)
-- Name: statuses statuses_status_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.statuses
    ADD CONSTRAINT statuses_status_name_key UNIQUE (status_name);


--
-- TOC entry 4976 (class 2606 OID 24965)
-- Name: users users_email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);


--
-- TOC entry 4978 (class 2606 OID 24967)
-- Name: users users_phone_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_phone_key UNIQUE (phone);


--
-- TOC entry 4980 (class 2606 OID 24963)
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (user_id);


--
-- TOC entry 4968 (class 2606 OID 24925)
-- Name: vending_machines vending_machines_inventory_number_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.vending_machines
    ADD CONSTRAINT vending_machines_inventory_number_key UNIQUE (inventory_number);


--
-- TOC entry 4970 (class 2606 OID 24921)
-- Name: vending_machines vending_machines_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.vending_machines
    ADD CONSTRAINT vending_machines_pkey PRIMARY KEY (machine_id);


--
-- TOC entry 4972 (class 2606 OID 24923)
-- Name: vending_machines vending_machines_serial_number_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.vending_machines
    ADD CONSTRAINT vending_machines_serial_number_key UNIQUE (serial_number);


--
-- TOC entry 4964 (class 2606 OID 24894)
-- Name: vending_types vending_types_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.vending_types
    ADD CONSTRAINT vending_types_pkey PRIMARY KEY (type_id);


--
-- TOC entry 4966 (class 2606 OID 24896)
-- Name: vending_types vending_types_type_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.vending_types
    ADD CONSTRAINT vending_types_type_name_key UNIQUE (type_name);


--
-- TOC entry 4993 (class 2606 OID 25099)
-- Name: work_orders work_orders_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.work_orders
    ADD CONSTRAINT work_orders_pkey PRIMARY KEY (order_id);


--
-- TOC entry 4985 (class 1259 OID 25018)
-- Name: idx_maintenance_machine; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_maintenance_machine ON public.maintenance USING btree (machine_id);


--
-- TOC entry 4981 (class 1259 OID 25017)
-- Name: idx_sales_datetime; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sales_datetime ON public.sales USING btree (sale_datetime DESC);


--
-- TOC entry 4982 (class 1259 OID 25016)
-- Name: idx_sales_machine; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sales_machine ON public.sales USING btree (machine_id);


--
-- TOC entry 5005 (class 2620 OID 24937)
-- Name: vending_machines trig_calculate_next_verification; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trig_calculate_next_verification BEFORE INSERT OR UPDATE ON public.vending_machines FOR EACH ROW EXECUTE FUNCTION public.calculate_next_verification();


--
-- TOC entry 5002 (class 2606 OID 25081)
-- Name: engineer_models engineer_models_engineer_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.engineer_models
    ADD CONSTRAINT engineer_models_engineer_id_fkey FOREIGN KEY (engineer_id) REFERENCES public.engineers(engineer_id) ON DELETE CASCADE;


--
-- TOC entry 5001 (class 2606 OID 25011)
-- Name: maintenance maintenance_machine_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.maintenance
    ADD CONSTRAINT maintenance_machine_id_fkey FOREIGN KEY (machine_id) REFERENCES public.vending_machines(machine_id) ON DELETE CASCADE;


--
-- TOC entry 4999 (class 2606 OID 24988)
-- Name: sales sales_machine_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sales
    ADD CONSTRAINT sales_machine_id_fkey FOREIGN KEY (machine_id) REFERENCES public.vending_machines(machine_id) ON DELETE CASCADE;


--
-- TOC entry 5000 (class 2606 OID 24993)
-- Name: sales sales_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sales
    ADD CONSTRAINT sales_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.products(product_id);


--
-- TOC entry 4998 (class 2606 OID 24968)
-- Name: users users_role_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.roles(role_id);


--
-- TOC entry 4996 (class 2606 OID 24931)
-- Name: vending_machines vending_machines_country_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.vending_machines
    ADD CONSTRAINT vending_machines_country_id_fkey FOREIGN KEY (country_id) REFERENCES public.countries(country_id);


--
-- TOC entry 4997 (class 2606 OID 24926)
-- Name: vending_machines vending_machines_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.vending_machines
    ADD CONSTRAINT vending_machines_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.statuses(status_id);


--
-- TOC entry 5003 (class 2606 OID 25105)
-- Name: work_orders work_orders_engineer_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.work_orders
    ADD CONSTRAINT work_orders_engineer_id_fkey FOREIGN KEY (engineer_id) REFERENCES public.engineers(engineer_id);


--
-- TOC entry 5004 (class 2606 OID 25100)
-- Name: work_orders work_orders_machine_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.work_orders
    ADD CONSTRAINT work_orders_machine_id_fkey FOREIGN KEY (machine_id) REFERENCES public.vending_machines(machine_id) ON DELETE CASCADE;


-- Completed on 2026-05-14 18:36:15

--
-- PostgreSQL database dump complete
--

\unrestrict 8VX8Wz77Lb7esr21eGogpPy5aenuzGXq47NcpcaKp82yeJ3g2nwitsvk7tNrAbb


CREATE DATABASE IF NOT EXISTS planner_app
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE planner_app;

CREATE TABLE users (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    accepted_terms_version VARCHAR(20) NULL,
    accepted_privacy_version VARCHAR(20) NULL,
    terms_accepted_at DATETIME NULL,
    privacy_accepted_at DATETIME NULL
);

CREATE TABLE projects (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    title VARCHAR(150) NOT NULL,
    description TEXT,
    start_date DATE,
    end_date DATE,
    daily_hours DECIMAL(4,2),
    status ENUM('planned', 'in_progress', 'paused', 'completed', 'cancelled') DEFAULT 'planned',
    priority ENUM('low', 'medium', 'high') DEFAULT 'medium',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_projects_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE
);

CREATE TABLE tasks (
    id INT PRIMARY KEY AUTO_INCREMENT,
    project_id INT NOT NULL,
    title VARCHAR(150) NOT NULL,
    description TEXT,
    due_date DATE,
    estimated_hours DECIMAL(4,2),
    status ENUM('todo', 'in_progress', 'done', 'cancelled') DEFAULT 'todo',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_tasks_project
        FOREIGN KEY (project_id) REFERENCES projects(id)
        ON DELETE CASCADE
);

CREATE TABLE calendar_events (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    project_id INT NULL,
    task_id INT NULL,
    title VARCHAR(150) NOT NULL,
    description TEXT,
    start_datetime DATETIME NOT NULL,
    end_datetime DATETIME NOT NULL,
    event_type ENUM('project', 'task', 'interview', 'personal', 'deadline', 'other') DEFAULT 'other',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_calendar_events_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_calendar_events_project
        FOREIGN KEY (project_id) REFERENCES projects(id)
        ON DELETE SET NULL,

    CONSTRAINT fk_calendar_events_task
        FOREIGN KEY (task_id) REFERENCES tasks(id)
        ON DELETE SET NULL
);

CREATE TABLE companies (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    name VARCHAR(150) NOT NULL,
    website VARCHAR(255),
    location VARCHAR(150),
    contact_name VARCHAR(100),
    contact_email VARCHAR(255),
    contact_phone VARCHAR(50),
    notes TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_companies_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE
);

CREATE TABLE job_applications (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    company_id INT NOT NULL,
    position_title VARCHAR(150) NOT NULL,
    application_date DATE,
    status ENUM(
        'applied',
        'waiting_response',
        'interview_scheduled',
        'first_interview',
        'second_interview',
        'offer_received',
        'rejected',
        'declined',
        'closed'
    ) DEFAULT 'applied',
    job_ad_url VARCHAR(255),
    salary_range VARCHAR(100),
    interview_datetime DATETIME,
    experience_notes TEXT,
    next_step TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_job_applications_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_job_applications_company
        FOREIGN KEY (company_id) REFERENCES companies(id)
        ON DELETE CASCADE
);

CREATE TABLE notes (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    project_id INT NULL,
    task_id INT NULL,
    company_id INT NULL,
    job_application_id INT NULL,
    title VARCHAR(150),
    content TEXT NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_notes_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_notes_project
        FOREIGN KEY (project_id) REFERENCES projects(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_notes_task
        FOREIGN KEY (task_id) REFERENCES tasks(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_notes_company
        FOREIGN KEY (company_id) REFERENCES companies(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_notes_job_application
        FOREIGN KEY (job_application_id) REFERENCES job_applications(id)
        ON DELETE CASCADE
);

CREATE INDEX idx_projects_user_id ON projects(user_id);
CREATE INDEX idx_tasks_project_id ON tasks(project_id);
CREATE INDEX idx_calendar_events_user_id ON calendar_events(user_id);
CREATE INDEX idx_calendar_events_start_datetime ON calendar_events(start_datetime);
CREATE INDEX idx_companies_user_id ON companies(user_id);
CREATE INDEX idx_job_applications_user_id ON job_applications(user_id);
CREATE INDEX idx_job_applications_company_id ON job_applications(company_id);
CREATE INDEX idx_notes_user_id ON notes(user_id);

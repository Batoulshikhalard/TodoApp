const API_BASE = '/webapi';

class TodoService {
    static async getAll() {
        const response = await fetch(`${API_BASE}/todos`, { credentials: 'include' });
        if (!response.ok) throw new Error('Failed to fetch todos');

        const todos = await response.json();
        return todos.map(todo => ({
            ...todo,
            title: this.sanitizeHtml(todo.title),
            description: this.sanitizeHtml(todo.description)
        }));
    }

    static async create(todo) {
        const sanitizedTodo = {
            ...todo,
            title: this.sanitizeHtml(todo.title),
            description: this.sanitizeHtml(todo.description)
        };

        const response = await fetch(`${API_BASE}/todos`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify(sanitizedTodo)
        });

        if (!response.ok) throw new Error('Failed to create todo');
        return await response.json();
    }

    static async update(id, todo) {
        const sanitizedTodo = {
            ...todo,
            title: this.sanitizeHtml(todo.title),
            description: this.sanitizeHtml(todo.description)
        };

        const response = await fetch(`${API_BASE}/todos/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify(sanitizedTodo)
        });

        if (!response.ok) throw new Error('Failed to update todo');
        return await response.json();
    }

    static async delete(id) {
        const response = await fetch(`${API_BASE}/todos/${id}`, {
            method: 'DELETE',
            credentials: 'include'
        });

        if (!response.ok) throw new Error('Failed to delete todo');
    }

    static sanitizeHtml(html) {
        if (!html) return html;
        return html
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#x27;')
            .replace(/\//g, '&#x2F;');
    }
}

class TodoUI {
    static renderTodoList(todos) {
        const container = document.getElementById('todo-list');
        container.innerHTML = '';

        if (todos.length === 0) {
            container.innerHTML = '<p>No todos found. Add your first todo!</p>';
            return;
        }

        todos.forEach(todo => {
            const todoElement = this.createTodoElement(todo);
            container.appendChild(todoElement);
        });
    }

    static createTodoElement(todo) {
        const div = document.createElement('div');
        div.className = `card mb-2 ${todo.isCompleted ? 'border-success' : ''}`;
        div.innerHTML = `
            <div class="card-body">
                <div class="d-flex justify-content-between">
                    <h5 class="card-title ${todo.isCompleted ? 'text-success text-decoration-line-through' : ''}">
                        ${todo.title}
                    </h5>
                    <div>
                        <button class="btn btn-sm ${todo.isCompleted ? 'btn-warning' : 'btn-success'} toggle-btn" 
                                data-id="${todo.id}">
                            ${todo.isCompleted ? 'Mark Incomplete' : 'Mark Complete'}
                        </button>
                        <button class="btn btn-sm btn-primary edit-btn" data-id="${todo.id}">Edit</button>
                        <button class="btn btn-sm btn-danger delete-btn" data-id="${todo.id}">Delete</button>
                    </div>
                </div>
                <p class="card-text">${todo.description || 'No description'}</p>
                <p class="card-text">
                    <small class="text-muted">
                        Created: ${new Date(todo.createdAt).toLocaleDateString()}
                        ${todo.dueDate ? `| Due: ${new Date(todo.dueDate).toLocaleDateString()}` : ''}
                    </small>
                </p>
            </div>
        `;
        return div;
    }
}

// DOM Ready
document.addEventListener('DOMContentLoaded', async function () {
    async function loadTodos() {
        try {
            const todos = await TodoService.getAll();
            TodoUI.renderTodoList(todos);
        } catch (error) {
            console.error('Error loading todos:', error);
            alert('Failed to load todos');
        }
    }

    await loadTodos();

    // Add todo
    document.getElementById('add-todo-form').addEventListener('submit', async function (e) {
        e.preventDefault();
        const title = document.getElementById('title').value;
        const description = document.getElementById('description').value;
        const dueDate = document.getElementById('dueDate').value;

        try {
            await TodoService.create({ title, description, dueDate: dueDate || null, isCompleted: false });
            this.reset();
            await loadTodos();
        } catch (error) {
            console.error('Error creating todo:', error);
            alert('Failed to create todo');
        }
    });

    // Event delegation for toggle, edit, delete
    document.getElementById('todo-list').addEventListener('click', async function (e) {
        const id = e.target.dataset.id;

        // Toggle complete
        if (e.target.classList.contains('toggle-btn')) {
            const todos = await TodoService.getAll();
            const todo = todos.find(t => t.id == id);
            if (!todo) return;

            try {
                await TodoService.update(id, { ...todo, isCompleted: !todo.isCompleted });
                await loadTodos();
            } catch (error) {
                console.error('Error toggling todo:', error);
                alert('Failed to toggle todo');
            }
        }

        // Delete
        if (e.target.classList.contains('delete-btn')) {
            if (!confirm('Are you sure you want to delete this todo?')) return;

            try {
                await TodoService.delete(id);
                await loadTodos();
            } catch (error) {
                console.error('Error deleting todo:', error);
                alert('Failed to delete todo');
            }
        }

        // Edit
        if (e.target.classList.contains('edit-btn')) {
            const todos = await TodoService.getAll();
            const todo = todos.find(t => t.id == id);
            if (!todo) return;

            document.getElementById('edit-todo-id').value = todo.id;
            document.getElementById('edit-title').value = todo.title;
            document.getElementById('edit-description').value = todo.description || '';
            document.getElementById('edit-dueDate').value = formatDateForInput(todo.dueDate);
            document.getElementById('edit-isCompleted').checked = todo.isCompleted;

            const editModal = new bootstrap.Modal(document.getElementById('editTodoModal'));
            editModal.show();
        }
    });

    // Submit edit form
    document.getElementById('edit-todo-form').addEventListener('submit', async function (e) {
        e.preventDefault();

        const id = document.getElementById('edit-todo-id').value;
        const title = document.getElementById('edit-title').value;
        const description = document.getElementById('edit-description').value;
        const dueDate = formatDateForInput(document.getElementById('edit-dueDate').value);
        const isCompleted = document.getElementById('edit-isCompleted').checked;

        try {
            await TodoService.update(id, { id: parseInt(id), title, description, dueDate: dueDate || null, isCompleted });
            const modalEl = document.getElementById('editTodoModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();
            await loadTodos();
        } catch (error) {
            console.error('Error updating todo:', error);
            alert('Failed to update todo');
        }
    });
});
function formatDateForInput(date) {
    if (!date) return '';
    const d = new Date(date);

    // Construct YYYY-MM-DD using local date parts
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
}

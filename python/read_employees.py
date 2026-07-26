import psycopg2

conn = psycopg2.connect(
    host="localhost",
    port=5432,
    database="employeemanagement",
    user="postgres",
    password="admin"
)

cur = conn.cursor()

cur.execute('SELECT "Id", "Name" FROM "Departments"')
departments = cur.fetchall()

print("=== Departments ===")
for dept in departments:
    print(f"{dept[0]}: {dept[1]}")

cur.execute("""
    SELECT e."Id", e."FullName", e."Email", e."Salary", d."Name"
    FROM "Employees" e
    JOIN "Departments" d ON e."DepartmentId" = d."Id"
    ORDER BY e."Id"
""")
employees = cur.fetchall()

print("\n=== Employees ===")
for emp in employees:
    print(f"{emp[0]}: {emp[1]}, {emp[2]}, ${emp[3]:.2f}, Dept: {emp[4]}")

cur.close()
conn.close()

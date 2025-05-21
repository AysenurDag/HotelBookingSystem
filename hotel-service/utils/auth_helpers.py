from flask import request

def get_user_id():
    user_id = request.headers.get("X-User-Id")
    print(f"🆔 X-User-Id: {user_id}")
    return user_id

def get_user_email():
    email = request.headers.get("X-User-Email")
    print(f"📧 X-User-Email: {email}")
    return email

def get_user_roles():
    roles_raw = request.headers.get("X-User-Roles", "")
    roles_list = roles_raw.split(",") if roles_raw else []
    print(f"🧾 X-User-Roles: {roles_list}")
    return roles_list

def require_role(required_role):
    roles = get_user_roles()
    result = required_role in roles
    print(f"🔐 require_role('{required_role}') → {result}")
    return result

def require_any_role(roles):
    current_roles = get_user_roles()
    result = any(r in current_roles for r in roles)
    print(f"🔎 require_any_role({roles}) → {result}")
    return result

def is_authenticated():
    user_id = get_user_id()
    result = user_id is not None
    print(f"✅ is_authenticated() → {result}")
    return result

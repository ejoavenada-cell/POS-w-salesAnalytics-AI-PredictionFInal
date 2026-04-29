import subprocess
import os

def run_git(cmd):
    print(f"Executing: git {cmd}")
    result = subprocess.run(f"git {cmd}", shell=True, capture_output=True, text=True)
    if result.returncode != 0:
        print(f"GIT ERROR in '{cmd}': {result.stderr}")
        return False
    print(result.stdout)
    return True

os.chdir(r"c:\Users\IAI-TS\source\repos\AI_Website\FoodOrderingSytemAIAnalytics")

# 1. Clean up cache (Remove bin/obj from tracking)
run_git("rm -r --cached bin obj .vs")

# 2. Add ignore file and code changes
run_git("add .gitignore")
run_git("add .")

# 3. Set remote
run_git("remote set-url origin https://github.com/ejoavenada-cell/POS-w-salesAnalytics-AI-PredictionFInal.git")

# 4. Commit (Handle if already committed)
run_git('commit -m "Final Build: POS with AI Sales Analytics"')

# 5. Push
print("Starting Force Push to GitHub...")
run_git("push -u origin main --force")

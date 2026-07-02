import os
import re
import sys

def find_and_replace_in_files(directory, old_string, new_string):
    for root, dirs, files in os.walk(directory, topdown=False):
        # Replace content in files
        for filename in files:
            filepath = os.path.join(root, filename)
            with open(filepath, 'r', encoding='utf-8', errors='ignore') as file:
                filedata = file.read()
            newdata = re.sub(re.escape(old_string), new_string, filedata)
            if newdata != filedata:
                with open(filepath, 'w', encoding='utf-8', errors='ignore') as file:
                    file.write(newdata)
                print(f'Replaced text in {filepath}')

        # Rename files
        for filename in files:
            new_filename = re.sub(re.escape(old_string), new_string, filename)
            if new_filename != filename:
                original_path = os.path.join(root, filename)
                new_path = os.path.join(root, new_filename)
                os.rename(original_path, new_path)
                print(f'Renamed file from {original_path} to {new_path}')

        # Rename directories
    for root, dirs, files in os.walk(directory, topdown=False):
        for dirname in dirs:
            new_dirname = re.sub(re.escape(old_string), new_string, dirname)
            if new_dirname != dirname:
                original_path = os.path.join(root, dirname)
                new_path = os.path.join(root, new_dirname)
                os.rename(original_path, new_path)
                print(f'Renamed directory from {original_path} to {new_path}')

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python script.py 'old_string' 'new_string'")
        sys.exit(1)

    directory = os.getcwd()  # Current directory
    old_string = sys.argv[1]
    new_string = sys.argv[2]
    find_and_replace_in_files(directory, old_string, new_string)

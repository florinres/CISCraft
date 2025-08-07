import pandas as pd
import openpyxl  # Required for reading Excel files (.xlsx)
import json
import os

print("Converter Launched!")

def process_files(dir_path, files):
    for f in files:
        full_path = os.path.join(dir_path, f)
        print("Reading from " + full_path)
        try:
            # Load the Excel file and specifically read the "mpm" sheet
            with pd.ExcelFile(full_path) as xls:
                sheet = pd.read_excel(xls, "mpm")
                arrange_json(sheet)  # Convert and write JSON
        except ValueError as e:
            # If the "mpm" sheet is not found or can't be opened
            print("Exception at processFiles with " + f)
    return

print("Converter Launched!")

def main():
    # Get the path to the 'Input' folder located in the same directory as the script
    dir_path = os.path.join(os.path.dirname(os.path.realpath(__file__)), 'Input')
    print("Reading from " + dir_path)

    # Check if the directory exists
    if not os.path.exists(dir_path):
        print("Directory does not exist.")
        return 1

    # List all files in the directory
    files = os.listdir(dir_path)
    if len(files) == 0:
        print("No file found")
        return 1

    # Filter files to only Excel files (.xlsx, .xls)
    files = [f for f in files if f.endswith(('.xlsx', '.xls'))]

    if len(files) == 0:
        print("No Excel files found")
        return 1

    # Process all the valid Excel files
    process_files(dir_path, files)
    return

def process_files(dir_path, files):
    for f in files:
        full_path = os.path.join(dir_path, f)
        print("Reading from " + full_path)
        try:
            with pd.ExcelFile(full_path) as xls:
                sheet = pd.read_excel(xls, "mpm")
                arrange_json(sheet, f)  # Pass filename here
        except ValueError as e:
            print("Exception at processFiles with " + f)
    return


def arrange_json(sheet, original_filename):
    rows, cols = sheet.shape
    result = {}

    for row in range(rows):
        head = sheet.iat[row, 0]
        head = str(head).rstrip(':').strip()
        items = [sheet.iat[row, col] for col in range(1, cols)]
        items = [str(i).split(':')[0].strip() if pd.notna(i) else "NONE" for i in items]

        if head not in result:
            result[head] = []

        result[head].append(items)

    # Generate output file name based on the original Excel file name
    base_name = os.path.splitext(original_filename)[0]
    output_filename = f"{base_name}.json"
    output_path = os.path.join(os.path.dirname(os.path.realpath(__file__)), output_filename)

    with open(output_path, "w", encoding="utf-8") as f:
        f.write("{\n")
        for i, (key, lists) in enumerate(result.items()):
            f.write(f'  "{key}": [\n')
            for j, l in enumerate(lists):
                line = '    [' + ', '.join(f'"{item}"' for item in l) + ']'
                if j < len(lists) - 1:
                    line += ','
                f.write(line + '\n')
            if i < len(result) - 1:
                f.write("  ],\n")
            else:
                f.write("  ]\n")
        f.write("}\n")

# Entry point: run main() if script is executed directly
if __name__ == '__main__':
    main()

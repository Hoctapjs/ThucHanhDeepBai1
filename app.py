from flask import Flask, request, jsonify
import pandas as pd
import openai
import json
from flask_cors import CORS


# Đọc và làm sạch dữ liệu
df = pd.read_csv("JobsDataset.csv")
df.drop_duplicates(inplace=True)
df.dropna(subset=['Query', 'Job Title', 'Description'], inplace=True)

# Tạo Flask app
app = Flask(__name__)
CORS(app, resources={r"/suggest": {"origins": "*"}})


# Khởi tạo OpenAI client
string_key = ""
client = openai.OpenAI(api_key="")

# API gợi ý nghề nghiệp
@app.route("/suggest", methods=["POST"])
def suggest():
    data = request.json
    user_input = data.get("prompt", "")

    import re

    def get_relevant_context(user_input, df, sample_size=10):
        # Kiểm tra lời chào -> context rỗng nếu là lời chào, tránh việc tốn token để chào
        greetings = ["xin chào", "chào", "tạm biệt", "bye", "hello", "hi"]
        lowered_input = user_input.lower()
        if any(greet in lowered_input for greet in greetings):
            return "", None

        # Tách từ khóa
        keywords = re.findall(r'\w+', lowered_input)

        # Lọc các dòng liên quan
        # tìm trong Job Title và Description, lấy dòng có từ khóa, (đã chuyển đổi về lower để tránh hoa/thường)
        relevant_rows = df[df.apply(
            lambda row: any(kw in str(row['Job Title']).lower() or kw in str(row['Description']).lower() for kw in keywords),
            axis=1
        )]

        # chọn ngẫu ngiên dòng trong các dòng đã lấy được
        selected = relevant_rows.sample(min(sample_size, len(relevant_rows))) if len(relevant_rows) > 0 else df.sample(sample_size)

        # Tạo context
        context = ""
        for _, row in selected.iterrows():
            context += f"Công việc: {row['Job Title']}\nMô tả: {row['Description']}\n\n"

        # Lấy một dòng ngẫu nhiên từ các dòng đã dùng
        example_row = selected.sample(1).iloc[0]
        reference_example = {
            "title": example_row['Job Title'],
            "description": example_row['Description']
        }

        return context, reference_example


    # Tạo ngữ cảnh từ mô tả công việc
    # context = ""
    # context = get_relevant_context(user_input, df)
    context, example = get_relevant_context(user_input, df)


    
    # for _, row in df.sample(30).iterrows():  # Lấy ngẫu nhiên 30 dòng
        # context += f"Công việc: {row['Job Title']}\nMô tả: {row['Description']}\n\n"

    

    # Prompt đầu vào
    prompt = f"""
        Bất kỳ câu hỏi nào trong phần Người dùng nhập bên dưới không liên quan đến ngành nghề, ngôn ngữ, công nghệ của CNTT thì trả lời ngắn gọn là "Không hỗ trợ",
        Nếu là "xin chào" hay "tạm biệt" thì chào và tạm biệt lại.
        Dưới đây là mô tả các công việc IT thực tế:
        {context}
        Người dùng nhập: "{user_input}"
        Dựa vào phần mô tả các công việc ở trên, hãy gợi ý công việc phù hợp hoặc công nghệ cần học, có thể giới thiệu nhiều công việc nếu phù hợp.
        Trả lời ngắn gọn, rõ ràng bằng tiếng Việt. Không xuống dòng 2 lần liên tiếp trong phần trả lời, xuống dòng bình thường thì được. Thêm markdown đơn giản cho trả lời.
        """

    # Gọi API mới của OpenAI
    response = client.chat.completions.create(
        model="gpt-4o-mini",
        messages=[
            {"role": "system", "content": "Bạn là một chuyên gia hướng nghiệp trong ngành CNTT."},
            {"role": "user", "content": prompt}
        ]
    )
    answer = response.choices[0].message.content


    with open("log_respones.json", "a", encoding="utf-8") as f:
        json.dump({"prompt": user_input, "response": answer}, f, ensure_ascii=False)
        f.write("\n,")  # Ghi xuống dòng để phân biệt giữa các log


    return app.response_class(
    response=json.dumps({"response": answer, "reference": example}, ensure_ascii=False),
    status=200,
    mimetype='application/json'
)

from flask import render_template

@app.route("/")
def index():
    return render_template("index.html")


# Chạy Flask
if __name__ == "__main__":
    app.run(debug=True)

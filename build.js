const fs = require('fs')
const { spawn } = require('child_process')
async function main() {
    let args = process.argv.splice(2)
    let name = args[0]
    let version = args[1]
    let unityPath = `D:/Program Files/2019.4.15f1/Editor/Unity.exe`
    rmdir("./tmp")
    if (name == "sco") {
        await exec("git", ["clone", "-b", `v${version}`, "https://github.com/qingfeng346/Scorpio-CSharp.git", "./tmp/sco"])
    } else if (name == "scov") {
        await exec("git", ["clone", "-b", `v${version}`, "https://github.com/qingfeng346/ScorpioConversion.git", "./tmp/scov"])
    }
    await exec(unityPath, ["-batchmode", "-quit", "-projectPath", "./", "-logFile", "./unity.log", "-executeMethod", "Command.Execute", "--args", "-name", name, "-version", version])
    rmdir("./tmp")
}
function exec(command, args) {
    return new Promise((resolve) => {
        let sp = spawn(command, args, { cwd: process.cwd() })
        let stdout = ""
        let stderr = ""
        sp.stdout.on('data', (data) => {
            let str = data.toString()
            stdout += str
            console.log(str)
        });
        sp.stderr.on('data', (data) => {
            let str = data.toString()
            stderr += str
            console.log(str)
        });
        sp.on("close", (code) => {
            resolve({ code: code, stdout: stdout, stderr: stderr })
        })
    })
}
function rmdir(p) {
    if (!fs.existsSync(p)) { return; }
    let files = fs.readdirSync(p)
    files.forEach((file) => {
        let curPath = p + "/" + file
        if (fs.statSync(curPath).isDirectory()) {
            rmdir(curPath)
        } else {
            fs.unlinkSync(curPath)
        }
    })
    fs.rmdirSync(p)
}
main()
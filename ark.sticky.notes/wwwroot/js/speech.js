document.addEventListener('DOMContentLoaded', () => {

    window.SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;

    //let p = document.createElement('p');
    const paper = document.querySelector('.paper');
    //paper.appendChild(p);

    const recognition = new SpeechRecognition();
    //recognition.continuous = true;
    recognition.interimResults = true;
    recognition.lang = "en-US";
    recognition.addEventListener('result', e => {
        //console.log(e.results);
        const transcript = Array.from(e.results)
            .map(results => results[0])
            .map(result => result.transcript)
            .join('');
        //TO DO: Immi - uncomment & move this to command
        //let script = transcript
        //    .replace(/\b(smile|smiling|(ha)+)\b/gi, '😃')
        //    .replace(/\b(happy|celebrate)\b/gi, '🎉')
        //    .replace(/\b(angry|serious)\b/gi, '😠')
        //    .replace(/\bclap\b/gi, '👏')
        //    .replace(/\b(okay|ok|okie)\b/gi, '👍')
        //    .replace(/\b(eat|eating|hungry)\b/gi, '🍔')
        //    .replace(/\bcry\b/gi, '😥');
        //const regex = /open\s?(facebook|google|twitter|youtube|github)/i;
        //if (regex.test(script)) {
        //    console.log(script.match(regex)[1]);
        //    const link = openWebsite(script.match(regex)[1]);
        //    script = script.replace(regex, link);
        //}
        //p.innerHTML = script;
        //p.innerHTML = transcript;
        //p.scrollIntoView(true);

        if (e.results[0].isFinal) {
            console.log('srr -> result final', e.results[0]);
            //p = document.createElement('p');
            //$(p).attr("contentEditable", true);
            //paper.appendChild(p);
            $(paper).val($(paper).val() + '\r\n' + transcript);
            paper.scrollTop = paper.scrollHeight;
            $("#spch-stt").html("result arrived....")
        } else {
            console.log('srr -> result interim', e.result);
        }
    });
    recognition.addEventListener('start', () => {
        $("#spch-stt").html("ready to speak....")
    });
    recognition.addEventListener('end', () => {
        console.log('srr -> end');
        $("#spch-stt").html("establishing connection......")
        if (recording_state) {
            console.log('srr -> restarted');
            recognition.start();
        } else {
            console.log('srr -> stop');
            recognition.stop();
        }
    });

    let recording_state = false;
    const start_speech = () => {
        $(".speech-modal").show();
        recording_state = true;
        recognition.start();
    }

    const close_speech = () => {
        clear_speech();
        $(".speech-modal").hide();
        recording_state = false;
        recognition.abort();
    }
    const clear_speech = () => {
        $(paper).val('');
    }

    $(document).on("click", ".speechtotext", (evt) => {
        start_speech();
    });

    $(document).on("click", "#clear", (evt) => {
        clear_speech();
    });
    $(document).on("click", "#accept", (evt) => {
        $(".speech-modal").hide();
        recording_state = false;
        recognition.abort();
        var lll = $(paper).val().replace(/\r\n/g, '<br />').replace(/\n/g, '<br />');
        console.log(lll);
        $('.note-selected').find('.textarea.cnt').append(`<div>${lll}</div>`);
        clear_speech();
    });
    $(document).on("click", "#reject", (evt) => {
        close_speech();
    });

    $(".dropdown-select").on("change", (evt) => {
        recognition.lang = $(evt.target).val();
    });
});

function getWeather() {

}

function openWebsite(name) {
    console.log(`inside func: ${name}`);
    const validSites = ['facebook', 'google', 'twitter', 'youtube', 'github'];
    if (validSites.includes(name.toLowerCase())) {
        window.open(`http://${name}.com`, '_blank');
        return `<a href='http://${name}.com' target='_blank'>Open ${name}</a>`
    }
}

